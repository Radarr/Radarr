using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class IdFixture : CoreTest<FileNameBuilder>
    {
        private Movie _movie;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>
                      .CreateNew()
                      .With(s => s.Title = "Movie Title")
                      .With(s => s.ImdbId = "tt12345")
                      .With(s => s.TmdbId = 123456)
                      .Build();

            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [Test]
        public void should_add_imdb_id()
        {
            _namingConfig.MovieFolderFormat = "{Movie Title} ({ImdbId})";

            Subject.GetMovieFolder(_movie)
                   .Should().Be($"Movie Title ({_movie.ImdbId})");
        }

        [Test]
        public void should_add_tmdb_id()
        {
            _namingConfig.MovieFolderFormat = "{Movie Title} ({TmdbId})";

            Subject.GetMovieFolder(_movie)
                   .Should().Be($"Movie Title ({_movie.TmdbId})");
        }

        [Test]
        public void should_add_imdb_tag()
        {
            _namingConfig.MovieFolderFormat = "{Movie Title} {imdb-{ImdbId}}";

            Subject.GetMovieFolder(_movie)
                   .Should().Be($"Movie Title {{imdb-{_movie.ImdbId}}}");
        }

        [TestCase("{Movie Title} {imdb-{ImdbId}}")]
        [TestCase("{Movie Title} {imdbid-{ImdbId}}")]
        [TestCase("{Movie Title} {{imdb-{ImdbId}}}")]
        [TestCase("{Movie Title} {{imdbid-{ImdbId}}}")]
        public void should_skip_imdb_tag_if_null(string movieFormat)
        {
            _namingConfig.MovieFolderFormat = movieFormat;

            _movie.ImdbId = null;

            Subject.GetMovieFolder(_movie)
                   .Should().Be("Movie Title");
        }

        [TestCase("{Movie Title} {{imdb-{ImdbId}}}")]
        public void should_handle_imdb_tag_curly_brackets(string movieFormat)
        {
            _namingConfig.MovieFolderFormat = movieFormat;

            Subject.GetMovieFolder(_movie)
                .Should().Be($"Movie Title {{{{imdb-{_movie.ImdbId}}}}}");
        }

        [TestCase("{Movie Title} {{tmdb-{TmdbId}}}")]
        public void should_handle_tmdb_tag_curly_brackets(string movieFormat)
        {
            _namingConfig.MovieFolderFormat = movieFormat;

            Subject.GetMovieFolder(_movie)
                .Should().Be($"Movie Title {{{{tmdb-{_movie.TmdbId}}}}}");
        }
    }
}
