using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.CompletedDownloadServiceTests
{
    [TestFixture]
    public class ProcessFixture : CoreTest<CompletedDownloadService>
    {
        private TrackedDownload _trackedDownload;

        [SetUp]
        public void Setup()
        {
            var completed = Builder<DownloadClientItem>.CreateNew()
                                                    .With(h => h.Status = DownloadItemStatus.Completed)
                                                    .With(h => h.OutputPath = new OsPath(@"C:\DropFolder\MyDownload".AsOsAgnostic()))
                                                    .With(h => h.Title = "Drone.S01E01.HDTV")
                                                    .Build();

            var remoteAlbum = BuildRemoteAlbum();

            _trackedDownload = Builder<TrackedDownload>.CreateNew()
                    .With(c => c.State = TrackedDownloadState.Downloading)
                    .With(c => c.DownloadItem = completed)
                    .With(c => c.RemoteBook = remoteAlbum)
                    .Build();

            Mocker.GetMock<IDownloadClient>()
              .SetupGet(c => c.Definition)
              .Returns(new DownloadClientDefinition { Id = 1, Name = "testClient" });

            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(c => c.Get(It.IsAny<int>()))
                  .Returns(Mocker.GetMock<IDownloadClient>().Object);

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.MostRecentForDownloadId(_trackedDownload.DownloadItem.DownloadId))
                  .Returns(new History.History());

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetArtist("Drone.S01E01.HDTV"))
                  .Returns(remoteAlbum.Author);
        }

        private RemoteBook BuildRemoteAlbum()
        {
            return new RemoteBook
            {
                Author = new Author(),
                Books = new List<Book> { new Book { Id = 1 } }
            };
        }

        private void GivenNoGrabbedHistory()
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.MostRecentForDownloadId(_trackedDownload.DownloadItem.DownloadId))
                .Returns((History.History)null);
        }

        private void GivenArtistMatch()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetArtist(It.IsAny<string>()))
                  .Returns(_trackedDownload.RemoteBook.Author);
        }

        private void GivenABadlyNamedDownload()
        {
            _trackedDownload.DownloadItem.DownloadId = "1234";
            _trackedDownload.DownloadItem.Title = "Droned Pilot"; // Set a badly named download
            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.MostRecentForDownloadId(It.Is<string>(i => i == "1234")))
                  .Returns(new History.History() { SourceTitle = "Droned S01E01" });

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetArtist(It.IsAny<string>()))
                  .Returns((Author)null);

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetArtist("Droned S01E01"))
                  .Returns(BuildRemoteAlbum().Author);
        }

        [TestCase(DownloadItemStatus.Downloading)]
        [TestCase(DownloadItemStatus.Failed)]
        [TestCase(DownloadItemStatus.Queued)]
        [TestCase(DownloadItemStatus.Paused)]
        [TestCase(DownloadItemStatus.Warning)]
        public void should_not_process_if_download_status_isnt_completed(DownloadItemStatus status)
        {
            _trackedDownload.DownloadItem.Status = status;

            Subject.Check(_trackedDownload);

            AssertNotReadyToImport();
        }

        [Test]
        public void should_not_process_if_matching_history_is_not_found_and_no_category_specified()
        {
            _trackedDownload.DownloadItem.Category = null;
            GivenNoGrabbedHistory();

            Subject.Check(_trackedDownload);

            AssertNotReadyToImport();
        }

        [Test]
        public void should_process_if_matching_history_is_not_found_but_category_specified()
        {
            _trackedDownload.DownloadItem.Category = "tv";
            GivenNoGrabbedHistory();
            GivenArtistMatch();

            Subject.Check(_trackedDownload);

            AssertReadyToImport();
        }

        [Test]
        public void should_not_process_if_output_path_is_empty()
        {
            _trackedDownload.DownloadItem.OutputPath = default;

            Subject.Check(_trackedDownload);

            AssertNotReadyToImport();
        }

        [Test]
        public void should_not_process_if_the_download_cannot_be_tracked_using_the_source_title_as_it_was_initiated_externally()
        {
            GivenABadlyNamedDownload();
            _trackedDownload.RemoteBook.Author = null;

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.MostRecentForDownloadId(It.Is<string>(i => i == "1234")));

            Subject.Check(_trackedDownload);

            AssertNotReadyToImport();
        }

        [Test]
        public void should_not_process_when_there_is_a_title_mismatch()
        {
            _trackedDownload.RemoteBook.Author = null;
            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetArtist("Drone.S01E01.HDTV"))
                  .Returns((Author)null);

            Subject.Check(_trackedDownload);

            AssertNotReadyToImport();
        }

        private void AssertNotReadyToImport()
        {
            _trackedDownload.State.Should().NotBe(TrackedDownloadState.ImportPending);
        }

        private void AssertReadyToImport()
        {
            _trackedDownload.State.Should().Be(TrackedDownloadState.ImportPending);
        }
    }
}
