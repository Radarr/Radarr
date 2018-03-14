using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
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
        }

        private void GivenExistingSeries()
        {
            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(It.IsAny<int>()))
                  .Returns(_existingMovie);
        }

        [Test]
        public void should_update_movie_when_it_changes()
        {
            GivenExistingSeries();

            Subject.UpdateMovie(_fakeMovie);

            Mocker.GetMock<IMovieRepository>()
                  .Verify(v => v.Update(_fakeMovie), Times.Once());
        }
    }
}
