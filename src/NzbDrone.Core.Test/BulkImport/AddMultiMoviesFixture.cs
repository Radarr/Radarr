using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Moq;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies.Events;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.BulkImport
{
    [TestFixture]
    public class AddMultiMoviesFixture : CoreTest<MovieService>
    {
        private List<Movie> fakeMovies;

        [SetUp]
        public void Setup()
        {
            fakeMovies = Builder<Movie>.CreateListOfSize(3).BuildList();
            fakeMovies.ForEach(m =>
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

            var movies = Subject.AddMovies(fakeMovies);

            foreach (Movie movie in movies)
            {
                movie.Path.Should().NotBeNullOrEmpty();
            }

            //Subject.GetAllMovies().Should().HaveCount(3);
        }

        [Test]
        public void movies_added_should_ignore_already_added()
        {
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetMovieFolder(It.IsAny<Movie>(), null))
                  .Returns((Movie m, NamingConfig n) => m.Title);

            Mocker.GetMock<IMovieRepository>().Setup(s => s.All()).Returns(new List<Movie> { fakeMovies[0] });

            var movies = Subject.AddMovies(fakeMovies);

            Mocker.GetMock<IMovieRepository>().Verify(v => v.InsertMany(It.Is<List<Movie>>(l => l.Count == 2)));
        }

        [Test]
        public void movies_added_should_ignore_duplicates()
        {
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetMovieFolder(It.IsAny<Movie>(), null))
                  .Returns((Movie m, NamingConfig n) => m.Title);

            fakeMovies[2].TmdbId = fakeMovies[0].TmdbId;

            var movies = Subject.AddMovies(fakeMovies);

            Mocker.GetMock<IMovieRepository>().Verify(v => v.InsertMany(It.Is<List<Movie>>(l => l.Count == 2)));
        }

    }
}
