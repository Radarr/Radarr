using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MovieTests.MovieServiceTests
{
    [TestFixture]
    public class MovieFileAddedFixture : CoreTest<MovieService>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                   .With(m => m.MovieFileId = 0)
                                   .Build();
        }

        [Test]
        public void should_attach_file_of_the_primary_version_to_the_movie()
        {
            var movieFile = Builder<MovieFile>.CreateNew()
                                              .With(f => f.MovieVersionId = 0)
                                              .With(f => f.Movie = _movie)
                                              .Build();

            Subject.Handle(new MovieFileAddedEvent(movieFile));

            Mocker.GetMock<IMovieRepository>()
                  .Verify(v => v.Update(It.Is<Movie>(m => m.MovieFileId == movieFile.Id)), Times.Once());
        }

        [Test]
        public void should_not_attach_file_of_an_additional_version_to_the_movie()
        {
            var movieFile = Builder<MovieFile>.CreateNew()
                                              .With(f => f.MovieVersionId = 7)
                                              .With(f => f.Movie = _movie)
                                              .Build();

            Subject.Handle(new MovieFileAddedEvent(movieFile));

            Mocker.GetMock<IMovieRepository>()
                  .Verify(v => v.Update(It.IsAny<Movie>()), Times.Never());
        }
    }
}
