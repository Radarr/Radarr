using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Messaging.Commands;
using Moq;
using System.Collections.Generic;
using NzbDrone.Core.Music;
using FizzWare.NBuilder;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class RedownloadFailedDownloadServiceFixture : CoreTest<RedownloadFailedDownloadService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(x => x.AutoRedownloadFailed)
                .Returns(true);

            Mocker.GetMock<IAlbumService>()
                .Setup(x => x.GetAlbumsByArtist(It.IsAny<int>()))
                .Returns(Builder<Album>.CreateListOfSize(3).Build() as List<Album>);
        }

        [Test]
        public void should_skip_redownload_if_event_has_skipredownload_set()
        {
            var failedEvent = new DownloadFailedEvent {
                ArtistId = 1,
                AlbumIds = new List<int> { 1 },
                SkipReDownload = true
            };

            Subject.HandleAsync(failedEvent);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(x => x.Push(It.IsAny<Command>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()),
                        Times.Never());
        }

        [Test]
        public void should_skip_redownload_if_redownload_failed_disabled()
        {
            var failedEvent = new DownloadFailedEvent {
                ArtistId = 1,
                AlbumIds = new List<int> { 1 }
            };

            Mocker.GetMock<IConfigService>()
                .Setup(x => x.AutoRedownloadFailed)
                .Returns(false);

            Subject.HandleAsync(failedEvent);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(x => x.Push(It.IsAny<Command>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()),
                        Times.Never());
        }

        [Test]
        public void should_redownload_album_on_failure()
        {
            var failedEvent = new DownloadFailedEvent {
                ArtistId = 1,
                AlbumIds = new List<int> { 2 }
            };

            Subject.HandleAsync(failedEvent);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(x => x.Push(It.Is<AlbumSearchCommand>(c => c.AlbumIds.Count == 1 &&
                                                              c.AlbumIds[0] == 2),
                                    It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()),
                        Times.Once());

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(x => x.Push(It.IsAny<ArtistSearchCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()),
                        Times.Never());
        }

        [Test]
        public void should_redownload_multiple_albums_on_failure()
        {
            var failedEvent = new DownloadFailedEvent {
                ArtistId = 1,
                AlbumIds = new List<int> { 2, 3 }
            };

            Subject.HandleAsync(failedEvent);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(x => x.Push(It.Is<AlbumSearchCommand>(c => c.AlbumIds.Count == 2 &&
                                                              c.AlbumIds[0] == 2 &&
                                                              c.AlbumIds[1] == 3),
                                    It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()),
                        Times.Once());

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(x => x.Push(It.IsAny<ArtistSearchCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()),
                        Times.Never());
        }

        [Test]
        public void should_redownload_artist_on_failure()
        {
            // note that artist is set to have 3 albums in setup
            var failedEvent = new DownloadFailedEvent {
                ArtistId = 2,
                AlbumIds = new List<int> { 1, 2, 3 }
            };

            Subject.HandleAsync(failedEvent);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(x => x.Push(It.Is<ArtistSearchCommand>(c => c.ArtistId == failedEvent.ArtistId),
                                    It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()),
                        Times.Once());

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(x => x.Push(It.IsAny<AlbumSearchCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()),
                        Times.Never());
        }
    }
}
