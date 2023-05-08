using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Download.Aggregation.Aggregators;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateLanguagesFixture : CoreTest<AggregateLanguages>
    {
        private RemoteMovie _remoteMovie;
        private Movie _movie;
        private string _simpleReleaseTitle = "Series.Title.S01E01.xyz-RlsGroup";

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                       .With(m => m.MovieMetadata = new MovieMetadata
                       {
                           Title = "Some Movie",
                           OriginalLanguage = Language.English
                       })
                       .Build();

            _remoteMovie = Builder<RemoteMovie>.CreateNew()
                                                 .With(l => l.ParsedMovieInfo = null)
                                                 .With(l => l.Movie = _movie)
                                                 .With(l => l.Release = new ReleaseInfo())
                                                 .Build();
        }

        private ParsedMovieInfo GetParsedMovieInfo(List<Language> languages, string releaseTitle, string releaseTokens = "")
        {
            return new ParsedMovieInfo
                   {
                       Languages = languages,
                       ReleaseTitle = releaseTitle,
                       SimpleReleaseTitle = releaseTokens
                   };
        }

        [Test]
        public void should_return_existing_language_if_episode_title_does_not_have_language()
        {
            _remoteMovie.ParsedMovieInfo = GetParsedMovieInfo(new List<Language> { Language.Original }, _simpleReleaseTitle);

            Subject.Aggregate(_remoteMovie).Languages.Should().Contain(_movie.MovieMetadata.Value.OriginalLanguage);
        }

        [Test]
        public void should_return_parsed_language()
        {
            _remoteMovie.ParsedMovieInfo = GetParsedMovieInfo(new List<Language> { Language.French }, _simpleReleaseTitle);

            Subject.Aggregate(_remoteMovie).Languages.Should().Equal(_remoteMovie.ParsedMovieInfo.Languages);
        }

        [Test]
        public void should_exclude_language_that_is_part_of_episode_title_when_release_tokens_contains_episode_title()
        {
            var releaseTitle = "Series.Title.S01E01.Jimmy.The.Greek.xyz-RlsGroup";
            var releaseTokens = ".Jimmy.The.Greek.xyz-RlsGroup";

            _remoteMovie.Movie.Title = "Jimmy The Greek";
            _remoteMovie.ParsedMovieInfo = GetParsedMovieInfo(new List<Language> { Language.Greek }, releaseTitle, releaseTokens);

            Subject.Aggregate(_remoteMovie).Languages.Should().Equal(_movie.MovieMetadata.Value.OriginalLanguage);
        }

        [Test]
        public void should_remove_parsed_language_that_is_part_of_episode_title_when_release_tokens_contains_episode_title()
        {
            var releaseTitle = "Series.Title.S01E01.Jimmy.The.Greek.French.xyz-RlsGroup";
            var releaseTokens = ".Jimmy.The.Greek.French.xyz-RlsGroup";

            _remoteMovie.Movie.Title = "Jimmy The Greek";
            _remoteMovie.ParsedMovieInfo = GetParsedMovieInfo(new List<Language> { Language.Greek, Language.French }, releaseTitle, releaseTokens);

            Subject.Aggregate(_remoteMovie).Languages.Should().Equal(Language.French);
        }

        [Test]
        public void should_not_exclude_language_that_is_part_of_episode_title_when_release_tokens_does_not_contain_episode_title()
        {
            var releaseTitle = "Series.Title.S01E01.xyz-RlsGroup";
            var releaseTokens = ".xyz-RlsGroup";

            _remoteMovie.Movie.Title = "Jimmy The Greek";
            _remoteMovie.ParsedMovieInfo = GetParsedMovieInfo(new List<Language> { Language.Greek }, releaseTitle, releaseTokens);

            Subject.Aggregate(_remoteMovie).Languages.Should().Equal(Language.Greek);
        }

        [Test]
        public void should_use_reparse_language_after_determining_languages_that_are_in_episode_titles()
        {
            var releaseTitle = "Series.Title.S01E01.Jimmy.The.Greek.Greek.xyz-RlsGroup";
            var releaseTokens = ".Jimmy.The.Greek.Greek.xyz-RlsGroup";

            _remoteMovie.Movie.Title = "Jimmy The Greek";
            _remoteMovie.ParsedMovieInfo = GetParsedMovieInfo(new List<Language> { Language.Greek }, releaseTitle, releaseTokens);

            Subject.Aggregate(_remoteMovie).Languages.Should().Equal(Language.Greek);
        }
    }
}
