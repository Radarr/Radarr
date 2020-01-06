using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.BulkImport
{
    [TestFixture]
    public class AddMultiMoviesFixture : CoreTest<MovieService>
    {
        private List<Movie> _fakeMovies;

        [SetUp]
        public void Setup()
        {
            _fakeMovies = Builder<Movie>.CreateListOfSize(3).BuildList();
            _fakeMovies.ForEach(m =>
            {
                m.Path = null;
                m.RootFolderPath = @"C:\Test\TV";
            });
        }

        [Test]
        public void movies_added_event_should_have_proper_path()
        {
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetMovieFolder(It.IsAny<Movie>(), null))
                  .Returns((Movie m, NamingConfig n) => m.Title);

            Mocker.GetMock<IMovieRepository>().Setup(s => s.FindByTmdbId(It.IsAny<List<int>>()))
                .Returns(new List<Movie>());

            var movies = Subject.AddMovies(_fakeMovies);

            foreach (Movie movie in movies)
            {
                movie.Path.Should().NotBeNullOrEmpty();
            }

            // Subject.GetAllMovies().Should().HaveCount(3);
        }

        [Test]
        public void movies_added_should_ignore_already_added()
        {
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetMovieFolder(It.IsAny<Movie>(), null))
                  .Returns((Movie m, NamingConfig n) => m.Title);

            Mocker.GetMock<IMovieRepository>().Setup(s => s.FindByTmdbId(It.IsAny<List<int>>()))
                .Returns(new List<Movie> { _fakeMovies[0] });

            var movies = Subject.AddMovies(_fakeMovies);

            Mocker.GetMock<IMovieRepository>().Verify(v => v.InsertMany(It.Is<List<Movie>>(l => l.Count == 2)));
        }

        [Test]
        public void movies_added_should_ignore_duplicates()
        {
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetMovieFolder(It.IsAny<Movie>(), null))
                  .Returns((Movie m, NamingConfig n) => m.Title);

            Mocker.GetMock<IMovieRepository>().Setup(s => s.FindByTmdbId(It.IsAny<List<int>>()))
                .Returns(new List<Movie>());

            _fakeMovies[2].TmdbId = _fakeMovies[0].TmdbId;

            var movies = Subject.AddMovies(_fakeMovies);

            Mocker.GetMock<IMovieRepository>().Verify(v => v.InsertMany(It.Is<List<Movie>>(l => l.Count == 2)));
        }
    }
}
