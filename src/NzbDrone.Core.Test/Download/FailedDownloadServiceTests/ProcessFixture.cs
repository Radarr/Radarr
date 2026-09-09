using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.FailedDownloadServiceTests
{
    [TestFixture]
    public class ProcessFixture : CoreTest<FailedDownloadService>
    {
        private TrackedDownload _trackedDownload;
        private List<MovieHistory> _grabHistory;

        [SetUp]
        public void Setup()
        {
            var completed = Builder<DownloadClientItem>.CreateNew()
                                                    .With(h => h.Status = DownloadItemStatus.Completed)
                                                    .With(h => h.OutputPath = new OsPath(@"C:\DropFolder\MyDownload".AsOsAgnostic()))
                                                    .With(h => h.Title = "Drone.S01E01.HDTV")
                                                    .Build();

            _grabHistory = Builder<MovieHistory>.CreateListOfSize(2).BuildList();

            var remoteMovie = new RemoteMovie
            {
                Movie = new Movie(),
            };

            _trackedDownload = Builder<TrackedDownload>.CreateNew()
                    .With(c => c.State = TrackedDownloadState.Downloading)
                    .With(c => c.DownloadItem = completed)
                    .With(c => c.RemoteMovie = remoteMovie)
                    .Build();

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.Find(_trackedDownload.DownloadItem.DownloadId, MovieHistoryEventType.Grabbed))
                  .Returns(_grabHistory);
        }

        private void GivenNoGrabbedHistory()
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.Find(_trackedDownload.DownloadItem.DownloadId, MovieHistoryEventType.Grabbed))
                .Returns(new List<MovieHistory>());
        }

        private void GivenStalledTorrentTimeout(int minutes)
        {
            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.StalledTorrentTimeout)
                .Returns(minutes);
        }

        private void GivenStalledDownload(int minutesSinceProgress)
        {
            _trackedDownload.Protocol = DownloadProtocol.Torrent;
            _trackedDownload.IsStalled = false;
            _trackedDownload.LastProgressDate = DateTime.UtcNow.AddMinutes(-minutesSinceProgress);
            _trackedDownload.DownloadItem.Status = DownloadItemStatus.Warning;
        }

        [Test]
        public void should_not_fail_if_matching_history_is_not_found()
        {
            GivenNoGrabbedHistory();

            Subject.Check(_trackedDownload);

            AssertDownloadNotFailed();
        }

        [Test]
        public void should_warn_if_matching_history_is_not_found()
        {
            _trackedDownload.DownloadItem.Status = DownloadItemStatus.Failed;
            GivenNoGrabbedHistory();

            Subject.Check(_trackedDownload);

            _trackedDownload.StatusMessages.Should().NotBeEmpty();
        }

        [Test]
        public void should_not_warn_if_matching_history_is_not_found_and_not_failed()
        {
            _trackedDownload.DownloadItem.Status = DownloadItemStatus.Failed;
            GivenNoGrabbedHistory();

            Subject.Check(_trackedDownload);

            _trackedDownload.StatusMessages.Should().NotBeEmpty();
        }

        [Test]
        public void should_not_mark_stalled_download_as_failed_pending_if_timeout_is_disabled()
        {
            GivenStalledDownload(120);

            Subject.Check(_trackedDownload);

            _trackedDownload.State.Should().Be(TrackedDownloadState.Downloading);
            _trackedDownload.IsStalled.Should().BeFalse();
        }

        [Test]
        public void should_mark_stalled_download_as_failed_pending_after_timeout()
        {
            GivenStalledTorrentTimeout(60);
            GivenStalledDownload(120);

            Subject.Check(_trackedDownload);

            _trackedDownload.State.Should().Be(TrackedDownloadState.FailedPending);
            _trackedDownload.IsStalled.Should().BeTrue();
        }

        [Test]
        public void should_mark_stalled_download_as_failed_pending_if_downloading_without_progress()
        {
            GivenStalledTorrentTimeout(60);
            GivenStalledDownload(120);
            _trackedDownload.DownloadItem.Status = DownloadItemStatus.Downloading;

            Subject.Check(_trackedDownload);

            _trackedDownload.State.Should().Be(TrackedDownloadState.FailedPending);
            _trackedDownload.IsStalled.Should().BeTrue();
        }

        [Test]
        public void should_not_mark_stalled_download_as_failed_pending_before_timeout()
        {
            GivenStalledTorrentTimeout(60);
            GivenStalledDownload(30);

            Subject.Check(_trackedDownload);

            _trackedDownload.State.Should().Be(TrackedDownloadState.Downloading);
            _trackedDownload.IsStalled.Should().BeFalse();
        }

        [Test]
        public void should_not_mark_stalled_download_as_failed_pending_for_usenet()
        {
            GivenStalledTorrentTimeout(60);
            GivenStalledDownload(120);
            _trackedDownload.Protocol = DownloadProtocol.Usenet;

            Subject.Check(_trackedDownload);

            _trackedDownload.State.Should().Be(TrackedDownloadState.Downloading);
            _trackedDownload.IsStalled.Should().BeFalse();
        }

        [Test]
        public void should_not_mark_paused_download_as_failed_pending()
        {
            GivenStalledTorrentTimeout(60);
            GivenStalledDownload(120);
            _trackedDownload.DownloadItem.Status = DownloadItemStatus.Paused;

            Subject.Check(_trackedDownload);

            _trackedDownload.State.Should().Be(TrackedDownloadState.Downloading);
            _trackedDownload.IsStalled.Should().BeFalse();
        }

        [Test]
        public void should_warn_if_stalled_download_was_not_grabbed()
        {
            GivenStalledTorrentTimeout(60);
            GivenStalledDownload(120);
            GivenNoGrabbedHistory();

            Subject.Check(_trackedDownload);

            _trackedDownload.StatusMessages.Should().NotBeEmpty();
            _trackedDownload.State.Should().Be(TrackedDownloadState.Downloading);
            _trackedDownload.IsStalled.Should().BeFalse();
        }

        private void AssertDownloadNotFailed()
        {
            Mocker.GetMock<IEventAggregator>()
               .Verify(v => v.PublishEvent(It.IsAny<DownloadFailedEvent>()), Times.Never());

            _trackedDownload.State.Should().NotBe(TrackedDownloadState.Failed);
        }

        private void AssertDownloadFailed()
        {
            Mocker.GetMock<IEventAggregator>()
            .Verify(v => v.PublishEvent(It.IsAny<DownloadFailedEvent>()), Times.Once());

            _trackedDownload.State.Should().Be(TrackedDownloadState.Failed);
        }
    }
}
