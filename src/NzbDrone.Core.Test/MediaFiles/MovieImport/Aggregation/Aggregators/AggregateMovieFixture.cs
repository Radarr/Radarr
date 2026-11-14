using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Versions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateMovieFixture : CoreTest<AggregateMovie>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                   .With(m => m.Path = "/Movies/Movie (2022)")
                                   .Build();

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(_movie.Id))
                  .Returns(_movie);
        }

        [Test]
        public void should_reload_the_movie()
        {
            var localMovie = new LocalMovie { Movie = new Movie { Id = _movie.Id } };

            Subject.Aggregate(localMovie, null).Movie.Should().BeSameAs(_movie);
        }

        [Test]
        public void should_keep_the_version_being_imported()
        {
            var version = new MovieVersion { Id = 7, MovieId = _movie.Id, Path = "/Movies 4K/Movie (2022)" };
            var versionView = _movie.ForVersion(version);

            Mocker.GetMock<IMovieVersionService>()
                  .Setup(s => s.GetVersionView(_movie, 7))
                  .Returns(versionView);

            var localMovie = new LocalMovie { Movie = _movie.ForVersion(version) };

            var result = Subject.Aggregate(localMovie, null).Movie;

            result.MovieVersionId.Should().Be(7);
            result.Path.Should().Be("/Movies 4K/Movie (2022)");
        }
    }
}
