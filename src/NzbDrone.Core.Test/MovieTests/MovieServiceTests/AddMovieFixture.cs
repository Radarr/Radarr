using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MovieTests.MovieServiceTests
{
    [TestFixture]
    public class AddMovieFixture : CoreTest<MovieService>
    {
        private Movie _fakeMovie;

        [SetUp]
        public void Setup()
        {
            _fakeMovie = Builder<Movie>.CreateNew().Build();
        }

        [Test]
        public void movie_added_event_should_have_proper_path()
        {
            _fakeMovie.Path = null;
            _fakeMovie.RootFolderPath = @"C:\Test\Movies";

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetMovieFolder(_fakeMovie, null))
                  .Returns(_fakeMovie.Title);

            var series = Subject.AddMovie(_fakeMovie);

            series.Path.Should().NotBeNull();
        }
    }
}
