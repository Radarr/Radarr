using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download.Aggregation.Aggregators;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.TorrentRss;
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
        public void should_return_multi_languages_when_indexer_id_has_multi_languages_configuration()
        {
            var releaseTitle = "Series.Title.S01E01.MULTi.1080p.WEB.H265-RlsGroup";
            var indexerDefinition = new IndexerDefinition
            {
                Id = 1,
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.French.Id } }
            };
            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.Get(1))
                .Returns(indexerDefinition);

            _remoteMovie.ParsedMovieInfo = GetParsedMovieInfo(new List<Language> { }, releaseTitle);
            _remoteMovie.Release.IndexerId = 1;
            _remoteMovie.Release.Title = releaseTitle;

            Subject.Aggregate(_remoteMovie).Languages.Should().BeEquivalentTo(new List<Language> { _movie.MovieMetadata.Value.OriginalLanguage, Language.French });
            Mocker.GetMock<IIndexerFactory>().Verify(c => c.Get(1), Times.Once());
            Mocker.GetMock<IIndexerFactory>().VerifyNoOtherCalls();
        }

        [Test]
        public void should_return_multi_languages_from_indexer_with_id_when_indexer_id_and_name_are_set()
        {
            var releaseTitle = "Series.Title.S01E01.MULTi.1080p.WEB.H265-RlsGroup";
            var indexerDefinition1 = new IndexerDefinition
            {
                Id = 1,
                Name = "MyIndexer1",
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.French.Id } }
            };
            var indexerDefinition2 = new IndexerDefinition
            {
                Id = 2,
                Name = "MyIndexer2",
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.German.Id } }
            };

            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.Get(1))
                .Returns(indexerDefinition1);

            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.All())
                .Returns(new List<IndexerDefinition>() { indexerDefinition1, indexerDefinition2 });

            _remoteMovie.ParsedMovieInfo = GetParsedMovieInfo(new List<Language> { }, releaseTitle);
            _remoteMovie.Release.IndexerId = 1;
            _remoteMovie.Release.Indexer = "MyIndexer2";
            _remoteMovie.Release.Title = releaseTitle;

            Subject.Aggregate(_remoteMovie).Languages.Should().BeEquivalentTo(new List<Language> {  _movie.MovieMetadata.Value.OriginalLanguage, Language.French });
            Mocker.GetMock<IIndexerFactory>().Verify(c => c.Get(1), Times.Once());
            Mocker.GetMock<IIndexerFactory>().VerifyNoOtherCalls();
        }

        [Test]
        public void should_return_multi_languages_when_indexer_name_has_multi_languages_configuration()
        {
            var releaseTitle = "Series.Title.S01E01.MULTi.1080p.WEB.H265-RlsGroup";
            var indexerDefinition = new IndexerDefinition
            {
                Id = 1,
                Name = "MyIndexer (Prowlarr)",
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.French.Id } }
            };

            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.FindByName("MyIndexer (Prowlarr)"))
                .Returns(indexerDefinition);

            _remoteMovie.ParsedMovieInfo = GetParsedMovieInfo(new List<Language> { }, releaseTitle);
            _remoteMovie.Release.Indexer = "MyIndexer (Prowlarr)";
            _remoteMovie.Release.Title = releaseTitle;

            Subject.Aggregate(_remoteMovie).Languages.Should().BeEquivalentTo(new List<Language> {  _movie.MovieMetadata.Value.OriginalLanguage, Language.French });
            Mocker.GetMock<IIndexerFactory>().Verify(c => c.FindByName("MyIndexer (Prowlarr)"), Times.Once());
            Mocker.GetMock<IIndexerFactory>().VerifyNoOtherCalls();
        }

        [Test]
        public void should_return_multi_languages_when_release_as_unknown_as_default_language_and_indexer_has_multi_languages_configuration()
        {
            var releaseTitle = "Series.Title.S01E01.MULTi.1080p.WEB.H265-RlsGroup";
            var indexerDefinition = new IndexerDefinition
            {
                Id = 1,
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.French.Id } }
            };
            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.Get(1))
                .Returns(indexerDefinition);

            _remoteMovie.ParsedMovieInfo = GetParsedMovieInfo(new List<Language> { Language.Unknown }, releaseTitle);
            _remoteMovie.Release.IndexerId = 1;
            _remoteMovie.Release.Title = releaseTitle;

            Subject.Aggregate(_remoteMovie).Languages.Should().BeEquivalentTo(new List<Language> { _movie.MovieMetadata.Value.OriginalLanguage, Language.French });
            Mocker.GetMock<IIndexerFactory>().Verify(c => c.Get(1), Times.Once());
            Mocker.GetMock<IIndexerFactory>().VerifyNoOtherCalls();
        }

        [Test]
        public void should_return_original_when_indexer_has_no_multi_languages_configuration()
        {
            var releaseTitle = "Series.Title.S01E01.MULTi.1080p.WEB.H265-RlsGroup";
            var indexerDefinition = new IndexerDefinition
            {
                Id = 1,
                Settings = new TorrentRssIndexerSettings { }
            };
            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.Get(1))
                .Returns(indexerDefinition);

            _remoteMovie.ParsedMovieInfo = GetParsedMovieInfo(new List<Language> { }, releaseTitle);
            _remoteMovie.Release.IndexerId = 1;
            _remoteMovie.Release.Title = releaseTitle;

            Subject.Aggregate(_remoteMovie).Languages.Should().BeEquivalentTo(new List<Language> { _movie.MovieMetadata.Value.OriginalLanguage });
            Mocker.GetMock<IIndexerFactory>().Verify(c => c.Get(1), Times.Once());
            Mocker.GetMock<IIndexerFactory>().VerifyNoOtherCalls();
        }

        [Test]
        public void should_return_original_when_no_indexer_value()
        {
            var releaseTitle = "Series.Title.S01E01.MULTi.1080p.WEB.H265-RlsGroup";

            _remoteMovie.ParsedMovieInfo = GetParsedMovieInfo(new List<Language> { }, releaseTitle);
            _remoteMovie.Release.Title = releaseTitle;

            Subject.Aggregate(_remoteMovie).Languages.Should().BeEquivalentTo(new List<Language> {  _movie.MovieMetadata.Value.OriginalLanguage });
            Mocker.GetMock<IIndexerFactory>().VerifyNoOtherCalls();
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
