using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;

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
    }
}