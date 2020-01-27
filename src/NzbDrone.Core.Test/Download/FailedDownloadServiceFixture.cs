using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class FailedDownloadServiceFixture : CoreTest<FailedDownloadService>
    {
        private TrackedDownload _trackedDownload;
        private List<History.History> _grabHistory;

        [SetUp]
        public void Setup()
        {
            var completed = Builder<DownloadClientItem>.CreateNew()
                                                    .With(h => h.Status = DownloadItemStatus.Completed)
                                                    .With(h => h.OutputPath = new OsPath(@"C:\DropFolder\MyDownload".AsOsAgnostic()))
                                                    .With(h => h.Title = "Drone.S01E01.HDTV")
                                                    .Build();

            _grabHistory = Builder<History.History>.CreateListOfSize(2).BuildList();

            var remoteEpisode = new RemoteMovie
            {
                Movie = new Movie(),
            };

            _trackedDownload = Builder<TrackedDownload>.CreateNew()
                    .With(c => c.State = TrackedDownloadStage.Downloading)
                    .With(c => c.DownloadItem = completed)
                    .With(c => c.RemoteMovie = remoteEpisode)
                    .Build();

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.Find(_trackedDownload.DownloadItem.DownloadId, HistoryEventType.Grabbed))
                  .Returns(_grabHistory);
        }

        private void GivenNoGrabbedHistory()
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.Find(_trackedDownload.DownloadItem.DownloadId, HistoryEventType.Grabbed))
                .Returns(new List<History.History>());
        }

        [Test]
        public void should_not_fail_if_matching_history_is_not_found()
        {
            GivenNoGrabbedHistory();

            Subject.Process(_trackedDownload);

            AssertDownloadNotFailed();
        }

        [Test]
        public void should_warn_if_matching_history_is_not_found()
        {
            _trackedDownload.DownloadItem.Status = DownloadItemStatus.Failed;
            GivenNoGrabbedHistory();

            Subject.Process(_trackedDownload);

            _trackedDownload.StatusMessages.Should().NotBeEmpty();
        }

        [Test]
        public void should_not_warn_if_matching_history_is_not_found_and_not_failed()
        {
            _trackedDownload.DownloadItem.Status = DownloadItemStatus.Failed;
            GivenNoGrabbedHistory();

            Subject.Process(_trackedDownload);

            _trackedDownload.StatusMessages.Should().NotBeEmpty();
        }

        [Test]
        public void should_mark_failed_if_encrypted()
        {
            _trackedDownload.DownloadItem.IsEncrypted = true;

            Subject.Process(_trackedDownload);

            AssertDownloadFailed();
        }

        [Test]
        public void should_mark_failed_if_download_item_is_failed()
        {
            _trackedDownload.DownloadItem.Status = DownloadItemStatus.Failed;

            Subject.Process(_trackedDownload);

            AssertDownloadFailed();
        }

        [Test]
        public void should_include_tracked_download_in_message()
        {
            _trackedDownload.DownloadItem.Status = DownloadItemStatus.Failed;

            Subject.Process(_trackedDownload);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.Is<DownloadFailedEvent>(c => c.TrackedDownload != null)), Times.Once());

            AssertDownloadFailed();
        }

        private void AssertDownloadNotFailed()
        {
            Mocker.GetMock<IEventAggregator>()
               .Verify(v => v.PublishEvent(It.IsAny<DownloadFailedEvent>()), Times.Never());

            _trackedDownload.State.Should().NotBe(TrackedDownloadStage.DownloadFailed);
        }

        private void AssertDownloadFailed()
        {
            Mocker.GetMock<IEventAggregator>()
            .Verify(v => v.PublishEvent(It.IsAny<DownloadFailedEvent>()), Times.Once());

            _trackedDownload.State.Should().Be(TrackedDownloadStage.DownloadFailed);
        }
    }
}
