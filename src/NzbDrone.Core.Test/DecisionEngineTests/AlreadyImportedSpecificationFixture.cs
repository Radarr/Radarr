using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.History;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class AlreadyImportedSpecificationFixture : CoreTest<AlreadyImportedSpecification>
    {
        private const int FIRST_MOVIE_ID = 1;
        private const string TITLE = "Movie.Title.2018.720p.HDTV.x264-Radarr";

        private Movie _movie;
        private MovieFile _movieFile;
        private QualityModel _hdtv720p;
        private QualityModel _hdtv1080p;
        private RemoteMovie _remoteMovie;
        private List<MovieHistory> _history;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                    .With(m => m.Id = FIRST_MOVIE_ID)
                                    .Build();

            _movieFile = Builder<MovieFile>.CreateNew().With(m => m.MovieId = _movie.Id).Build();

            _movie.MovieFiles = new List<MovieFile> { _movieFile };

            _hdtv720p = new QualityModel(Quality.HDTV720p, new Revision(version: 1));
            _hdtv1080p = new QualityModel(Quality.HDTV1080p, new Revision(version: 1));

            _remoteMovie = new RemoteMovie
            {
                Movie = _movie,
                ParsedMovieInfo = new ParsedMovieInfo { Quality = _hdtv720p },
                Release = Builder<ReleaseInfo>.CreateNew()
                                              .Build()
            };

            _history = new List<MovieHistory>();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(true);

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.GetByMovieId(It.IsAny<int>(), null))
                  .Returns(_history);
        }

        private void GivenCdhDisabled()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(false);
        }

        private void GivenHistoryItem(string downloadId, string sourceTitle, QualityModel quality, MovieHistoryEventType eventType)
        {
            _history.Add(new MovieHistory
                         {
                             DownloadId = downloadId,
                             SourceTitle = sourceTitle,
                             Quality = quality,
                             Date = DateTime.UtcNow,
                             EventType = eventType
                         });
        }

        [Test]
        public void should_be_accepted_if_CDH_is_disabled()
        {
            GivenCdhDisabled();

            Subject.IsSatisfiedBy(_remoteMovie, null).Should().OnlyContain(x => x.Accepted);
        }

        [Test]
        public void should_be_accepted_if_movie_does_not_have_a_file()
        {
            _movie.MovieFiles = new List<MovieFile> { };

            Subject.IsSatisfiedBy(_remoteMovie, null).Should().OnlyContain(x => x.Accepted);
        }

        [Test]
        public void should_be_accepted_if_movie_does_not_have_grabbed_event()
        {
            Subject.IsSatisfiedBy(_remoteMovie, null).Should().OnlyContain(x => x.Accepted);
        }

        [Test]
        public void should_be_accepted_if_movie_does_not_have_imported_event()
        {
            GivenHistoryItem(Guid.NewGuid().ToString().ToUpper(), TITLE, _hdtv720p, MovieHistoryEventType.Grabbed);

            Subject.IsSatisfiedBy(_remoteMovie, null).Should().OnlyContain(x => x.Accepted);
        }

        [Test]
        public void should_be_accepted_if_grabbed_and_imported_quality_is_the_same()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(downloadId, TITLE, _hdtv720p, MovieHistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _hdtv720p, MovieHistoryEventType.DownloadFolderImported);

            Subject.IsSatisfiedBy(_remoteMovie, null).Should().OnlyContain(x => x.Accepted);
        }

        [Test]
        public void should_be_accepted_if_grabbed_download_id_and_release_torrent_hash_is_unknown()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(downloadId, TITLE, _hdtv720p, MovieHistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _hdtv1080p, MovieHistoryEventType.DownloadFolderImported);

            _remoteMovie.Release = Builder<TorrentInfo>.CreateNew()
                                                         .With(t => t.DownloadProtocol = DownloadProtocol.Torrent)
                                                         .With(t => t.InfoHash = null)
                                                         .Build();

            Subject.IsSatisfiedBy(_remoteMovie, null).Should().OnlyContain(x => x.Accepted);
        }

        [Test]
        public void should_be_accepted_if_grabbed_download_does_not_have_an_id()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(null, TITLE, _hdtv720p, MovieHistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _hdtv1080p, MovieHistoryEventType.DownloadFolderImported);

            _remoteMovie.Release = Builder<TorrentInfo>.CreateNew()
                                                         .With(t => t.DownloadProtocol = DownloadProtocol.Torrent)
                                                         .With(t => t.InfoHash = downloadId)
                                                         .Build();

            Subject.IsSatisfiedBy(_remoteMovie, null).Should().OnlyContain(x => x.Accepted);
        }

        [Test]
        public void should_be_rejected_if_grabbed_download_id_matches_release_torrent_hash()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(downloadId, TITLE, _hdtv720p, MovieHistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _hdtv1080p, MovieHistoryEventType.DownloadFolderImported);

            _remoteMovie.Release = Builder<TorrentInfo>.CreateNew()
                                                         .With(t => t.DownloadProtocol = DownloadProtocol.Torrent)
                                                         .With(t => t.InfoHash = downloadId)
                                                         .Build();

            Subject.IsSatisfiedBy(_remoteMovie, null).Should().OnlyContain(x => !x.Accepted);
        }

        [Test]
        public void should_be_rejected_if_release_title_matches_grabbed_event_source_title()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(downloadId, TITLE, _hdtv720p, MovieHistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _hdtv1080p, MovieHistoryEventType.DownloadFolderImported);

            _remoteMovie.Release = Builder<TorrentInfo>.CreateNew()
                                                         .With(t => t.DownloadProtocol = DownloadProtocol.Torrent)
                                                         .With(t => t.InfoHash = downloadId)
                                                         .Build();

            Subject.IsSatisfiedBy(_remoteMovie, null).Should().OnlyContain(x => !x.Accepted);
        }
    }
}
