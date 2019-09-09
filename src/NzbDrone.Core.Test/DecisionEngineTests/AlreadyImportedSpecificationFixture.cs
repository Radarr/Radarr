using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Music;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class AlreadyImportedSpecificationFixture : CoreTest<AlreadyImportedSpecification>
    {
        private const int FIRST_ALBUM_ID = 1;
        private const string TITLE = "Some.Artist-Some.Album-2018-320kbps-CD-Lidarr";

        private Artist _artist;
        private QualityModel _mp3;
        private QualityModel _flac;
        private RemoteAlbum _remoteAlbum;
        private List<History.History> _history;
        private TrackFile _firstFile;

        [SetUp]
        public void Setup()
        {
            var singleAlbumList = new List<Album>
                                    {
                                        new Album
                                        {
                                            Id = FIRST_ALBUM_ID,
                                            Title = "Some Album"
                                        }
                                    };

            _artist = Builder<Artist>.CreateNew()
                                     .Build();

            _firstFile = new TrackFile { Quality = new QualityModel(Quality.FLAC, new Revision(version: 2)), DateAdded = DateTime.Now };

            _mp3 = new QualityModel(Quality.MP3_320, new Revision(version: 1));
            _flac = new QualityModel(Quality.FLAC, new Revision(version: 1));

            _remoteAlbum = new RemoteAlbum
            {
                Artist = _artist,
                ParsedAlbumInfo = new ParsedAlbumInfo { Quality = _mp3 },
                Albums = singleAlbumList,
                Release = Builder<ReleaseInfo>.CreateNew()
                                              .Build()
            };

            _history = new List<History.History>();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(true);

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.GetByAlbum(It.IsAny<int>(), null))
                  .Returns(_history);

            Mocker.GetMock<IMediaFileService>()
                  .Setup(c => c.GetFilesByAlbum(It.IsAny<int>()))
                  .Returns(new List<TrackFile> { _firstFile });
        }

        private void GivenCdhDisabled()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(false);
        }

        private void GivenHistoryItem(string downloadId, string sourceTitle, QualityModel quality, HistoryEventType eventType)
        {
            _history.Add(new History.History
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

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_album_does_not_have_a_file()
        {
            Mocker.GetMock<IMediaFileService>()
                .Setup(c => c.GetFilesByAlbum(It.IsAny<int>()))
                .Returns(new List<TrackFile> { });

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_album_does_not_have_grabbed_event()
        {
            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_album_does_not_have_imported_event()
        {
            GivenHistoryItem(Guid.NewGuid().ToString().ToUpper(), TITLE, _mp3, HistoryEventType.Grabbed);

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_grabbed_and_imported_quality_is_the_same()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(downloadId, TITLE, _mp3, HistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _mp3, HistoryEventType.DownloadImported);

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_rejected_if_grabbed_download_id_matches_release_torrent_hash()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(downloadId, TITLE, _mp3, HistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _flac, HistoryEventType.DownloadImported);

            _remoteAlbum.Release = Builder<TorrentInfo>.CreateNew()
                                                         .With(t => t.DownloadProtocol = DownloadProtocol.Torrent)
                                                         .With(t => t.InfoHash = downloadId)
                                                         .Build();

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_accepted_if_release_torrent_hash_is_null()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(downloadId, TITLE, _mp3, HistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _flac, HistoryEventType.DownloadImported);

            _remoteAlbum.Release = Builder<TorrentInfo>.CreateNew()
                                                         .With(t => t.DownloadProtocol = DownloadProtocol.Torrent)
                                                         .With(t => t.InfoHash = null)
                                                         .Build();

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_release_torrent_hash_is_null_and_downloadId_is_null()
        {
            GivenHistoryItem(null, TITLE, _mp3, HistoryEventType.Grabbed);
            GivenHistoryItem(null, TITLE, _flac, HistoryEventType.DownloadImported);

            _remoteAlbum.Release = Builder<TorrentInfo>.CreateNew()
                                                         .With(t => t.DownloadProtocol = DownloadProtocol.Torrent)
                                                         .With(t => t.InfoHash = null)
                                                         .Build();

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_rejected_if_release_title_matches_grabbed_event_source_title()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(downloadId, TITLE, _mp3, HistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _flac, HistoryEventType.DownloadImported);

            _remoteAlbum.Release = Builder<TorrentInfo>.CreateNew()
                                                         .With(t => t.DownloadProtocol = DownloadProtocol.Torrent)
                                                         .With(t => t.InfoHash = downloadId)
                                                         .Build();

            Subject.IsSatisfiedBy(_remoteAlbum, null).Accepted.Should().BeFalse();
        }
    }
}
