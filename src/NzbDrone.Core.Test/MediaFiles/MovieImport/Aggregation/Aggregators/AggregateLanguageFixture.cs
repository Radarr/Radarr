using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.MovieImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateLanguageFixture : CoreTest<AggregateLanguage>
    {
        private LocalMovie _localMovie;

        [SetUp]
        public void Setup()
        {
            _localMovie = Builder<LocalMovie>.CreateNew()
                                                 .With(l => l.DownloadClientMovieInfo = null)
                                                 .With(l => l.FolderMovieInfo = null)
                                                 .With(l => l.FileMovieInfo = null)
                                                 .Build();
        }

        private ParsedMovieInfo GetParsedMovieInfo(List<Language> languages)
        {
            return new ParsedMovieInfo
            {
                Languages =  languages
            };
        }

        [Test]
        public void should_return_default_if_no_info_is_known()
        {
            Subject.Aggregate(_localMovie, false).Languages.Should().Contain(Language.English);
        }

        [Test]
        public void should_return_file_language_when_only_file_info_is_known()
        {
            _localMovie.FileMovieInfo = GetParsedMovieInfo(new List<Language> { Language.English });

            Subject.Aggregate(_localMovie, false).Languages.Should().Equal(_localMovie.FileMovieInfo.Languages);
        }

        [Test]
        public void should_return_folder_language_when_folder_info_is_known()
        {
            _localMovie.FolderMovieInfo = GetParsedMovieInfo(new List<Language> { Language.English });
            _localMovie.FileMovieInfo = GetParsedMovieInfo(new List<Language> { Language.English });

            var aggregation = Subject.Aggregate(_localMovie, false);

            aggregation.Languages.Should().Equal(_localMovie.FolderMovieInfo.Languages);
        }

        [Test]
        public void should_return_download_client_item_language_when_download_client_item_info_is_known()
        {
            _localMovie.DownloadClientMovieInfo = GetParsedMovieInfo(new List<Language> { Language.English });
            _localMovie.FolderMovieInfo = GetParsedMovieInfo(new List<Language> { Language.English });
            _localMovie.FileMovieInfo = GetParsedMovieInfo(new List<Language> { Language.English });

            Subject.Aggregate(_localMovie, false).Languages.Should().Equal(_localMovie.DownloadClientMovieInfo.Languages);
        }

        [Test]
        public void should_return_file_language_when_file_language_is_higher_than_others()
        {
            _localMovie.DownloadClientMovieInfo = GetParsedMovieInfo(new List<Language> { Language.English });
            _localMovie.FolderMovieInfo = GetParsedMovieInfo(new List<Language> { Language.English });
            _localMovie.FileMovieInfo = GetParsedMovieInfo(new List<Language> { Language.French });

            Subject.Aggregate(_localMovie, false).Languages.Should().Equal(_localMovie.FileMovieInfo.Languages);
        }

        [Test]
        public void should_return_multi_language()
        {
            _localMovie.DownloadClientMovieInfo = GetParsedMovieInfo(new List<Language> { Language.English });
            _localMovie.FolderMovieInfo = GetParsedMovieInfo(new List<Language> { Language.English, Language.German });
            _localMovie.FileMovieInfo = GetParsedMovieInfo(new List<Language> { Language.English });

            Subject.Aggregate(_localMovie, false).Languages.Should().Equal(_localMovie.FolderMovieInfo.Languages);
        }
    }
}
