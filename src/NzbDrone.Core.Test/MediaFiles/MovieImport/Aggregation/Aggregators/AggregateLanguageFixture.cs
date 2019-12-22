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

        private ParsedMovieInfo GetParsedMovieInfo(Language language)
        {
            return new ParsedMovieInfo
            {
                Languages = new List<Language> { language }
            };
        }

        [Test]
        public void should_return_file_language_when_only_file_info_is_known()
        {
            _localMovie.FileMovieInfo = GetParsedMovieInfo(Language.English);

            Subject.Aggregate(_localMovie, false).Languages.Should().Contain(_localMovie.FileMovieInfo.Languages);
        }

        [Test]
        public void should_return_folder_language_when_folder_info_is_known()
        {
            _localMovie.FolderMovieInfo = GetParsedMovieInfo(Language.English);
            _localMovie.FileMovieInfo = GetParsedMovieInfo(Language.English);

            Subject.Aggregate(_localMovie, false).Languages.Should().Contain(_localMovie.FolderMovieInfo.Languages);
        }

        [Test]
        public void should_return_download_client_item_language_when_download_client_item_info_is_known()
        {
            _localMovie.DownloadClientMovieInfo = GetParsedMovieInfo(Language.English);
            _localMovie.FolderMovieInfo = GetParsedMovieInfo(Language.English);
            _localMovie.FileMovieInfo = GetParsedMovieInfo(Language.English);

            Subject.Aggregate(_localMovie, false).Languages.Should().Contain(_localMovie.DownloadClientMovieInfo.Languages);
        }

        [Test]
        public void should_return_file_language_when_file_language_is_higher_than_others()
        {
            _localMovie.DownloadClientMovieInfo = GetParsedMovieInfo(Language.English);
            _localMovie.FolderMovieInfo = GetParsedMovieInfo(Language.English);
            _localMovie.FileMovieInfo = GetParsedMovieInfo(Language.French);

            Subject.Aggregate(_localMovie, false).Languages.Should().Contain(_localMovie.FileMovieInfo.Languages);
        }
    }
}
