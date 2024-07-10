using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MovieTests.MovieServiceTests
{
    [TestFixture]
    public class UpdateMovieFixture : CoreTest<MovieService>
    {
        private Movie _fakeMovie;
        private Movie _existingMovie;

        [SetUp]
        public void Setup()
        {
            _fakeMovie = Builder<Movie>.CreateNew().Build();
            _existingMovie = Builder<Movie>.CreateNew().Build();

            Mocker.GetMock<IAutoTaggingService>()
                .Setup(s => s.GetTagChanges(It.IsAny<Movie>()))
                .Returns(new AutoTaggingChanges());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.Update(It.IsAny<Movie>()))
                .Returns<Movie>(r => r);
        }

        private void GivenExistingMovie()
        {
            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(It.IsAny<int>()))
                  .Returns(_existingMovie);
        }

        [Test]
        public void should_update_movie_when_it_changes()
        {
            GivenExistingMovie();

            Subject.UpdateMovie(_fakeMovie);

            Mocker.GetMock<IMovieRepository>()
                  .Verify(v => v.Update(_fakeMovie), Times.Once());
        }

        [Test]
        public void should_add_and_remove_tags()
        {
            GivenExistingMovie();

            _fakeMovie.Tags = new HashSet<int> { 1, 2 };
            _fakeMovie.Monitored = false;

            Mocker.GetMock<IAutoTaggingService>()
                .Setup(s => s.GetTagChanges(_fakeMovie))
                .Returns(new AutoTaggingChanges
                {
                    TagsToAdd = new HashSet<int> { 3 },
                    TagsToRemove = new HashSet<int> { 1 }
                });

            var result = Subject.UpdateMovie(_fakeMovie);

            result.Tags.Should().BeEquivalentTo(new[] { 2, 3 });
        }
    }
}
