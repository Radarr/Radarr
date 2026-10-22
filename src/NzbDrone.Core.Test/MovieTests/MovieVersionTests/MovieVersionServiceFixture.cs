using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Movies.Versions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MovieTests.MovieVersionTests
{
    [TestFixture]
    public class MovieVersionServiceFixture : CoreTest<MovieVersionService>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                   .With(m => m.Id = 5)
                                   .With(m => m.QualityProfileId = 3)
                                   .With(m => m.Path = "/Movies/Movie (2020)")
                                   .With(m => m.MovieFileId = 0)
                                   .Build();
        }

        [Test]
        public void should_create_primary_version_when_movie_is_added()
        {
            Mocker.GetMock<IMovieVersionRepository>()
                  .Setup(s => s.FindByMovieId(_movie.Id))
                  .Returns(new List<MovieVersion>());

            Subject.Handle(new MovieAddedEvent(_movie));

            Mocker.GetMock<IMovieVersionRepository>()
                  .Verify(v => v.Insert(It.Is<MovieVersion>(m => m.MovieId == 5 && m.QualityProfileId == 3 && m.Path == "/Movies/Movie (2020)")),
                          Times.Once());
        }

        [Test]
        public void should_not_create_version_when_movie_already_has_one()
        {
            Mocker.GetMock<IMovieVersionRepository>()
                  .Setup(s => s.FindByMovieId(_movie.Id))
                  .Returns(new List<MovieVersion> { new MovieVersion { MovieId = _movie.Id } });

            Subject.Handle(new MovieAddedEvent(_movie));

            Mocker.GetMock<IMovieVersionRepository>()
                  .Verify(v => v.Insert(It.IsAny<MovieVersion>()), Times.Never());
        }

        [Test]
        public void should_delete_versions_when_movies_are_deleted()
        {
            Subject.HandleAsync(new MoviesDeletedEvent(new List<Movie> { _movie }, false, false));

            Mocker.GetMock<IMovieVersionRepository>()
                  .Verify(v => v.DeleteForMovies(It.Is<List<int>>(l => l.Count == 1 && l[0] == 5)), Times.Once());
        }
    }
}
