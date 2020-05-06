using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.TrackImport;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.CompletedDownloadServiceTests
{
    [TestFixture]
    public class ImportFixture : CoreTest<CompletedDownloadService>
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
                    .With(c => c.RemoteAlbum = remoteAlbum)
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
                  .Returns(remoteAlbum.Artist);
        }

        private Book CreateAlbum(int id)
        {
            return new Book
            {
                Id = id
            };
        }

        private RemoteAlbum BuildRemoteAlbum()
        {
            return new RemoteAlbum
            {
                Artist = new Author(),
                Albums = new List<Book> { CreateAlbum(1) }
            };
        }

        private void GivenABadlyNamedDownload()
        {
            _trackedDownload.RemoteAlbum.Artist = null;
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
                .Returns(BuildRemoteAlbum().Artist);
        }

        private void GivenArtistMatch()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetArtist(It.IsAny<string>()))
                  .Returns(_trackedDownload.RemoteAlbum.Artist);
        }

        [Test]
        public void should_not_mark_as_imported_if_all_files_were_rejected()
        {
            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Author>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision<LocalTrack>(
                                       new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic() }, new Rejection("Rejected!")), "Test Failure"),

                               new ImportResult(
                                   new ImportDecision<LocalTrack>(
                                       new LocalTrack { Path = @"C:\TestPath\Droned.S01E02.mkv".AsOsAgnostic() }, new Rejection("Rejected!")), "Test Failure")
                           });

            Subject.Import(_trackedDownload);

            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent<DownloadCompletedEvent>(It.IsAny<DownloadCompletedEvent>()), Times.Never());

            AssertNotImported();
        }

        [Test]
        public void should_not_mark_as_imported_if_no_tracks_were_parsed()
        {
            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Author>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision<LocalTrack>(
                                       new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic() }, new Rejection("Rejected!")), "Test Failure"),

                               new ImportResult(
                                   new ImportDecision<LocalTrack>(
                                       new LocalTrack { Path = @"C:\TestPath\Droned.S01E02.mkv".AsOsAgnostic() }, new Rejection("Rejected!")), "Test Failure")
                           });

            _trackedDownload.RemoteAlbum.Albums.Clear();

            Subject.Import(_trackedDownload);

            AssertNotImported();
        }

        [Test]
        public void should_not_mark_as_imported_if_all_files_were_skipped()
        {
            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Author>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic() }), "Test Failure"),
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic() }), "Test Failure")
                           });

            Subject.Import(_trackedDownload);

            AssertNotImported();
        }

        [Test]
        public void should_mark_as_imported_if_all_tracks_were_imported_but_extra_files_were_not()
        {
            GivenArtistMatch();

            _trackedDownload.RemoteAlbum.Albums = new List<Book>
            {
                CreateAlbum(1)
            };

            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Author>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic() })),
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic() }), "Test Failure")
                           });

            Subject.Import(_trackedDownload);

            AssertImported();
        }

        [Test]
        public void should_not_mark_as_imported_if_some_tracks_were_not_imported()
        {
            _trackedDownload.RemoteAlbum.Albums = new List<Book>
            {
                CreateAlbum(1),
                CreateAlbum(1),
                CreateAlbum(1)
            };

            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Author>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic() })),
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic() })),
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic() }), "Test Failure"),
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic() }), "Test Failure"),
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic() }), "Test Failure")
                           });

            var history = Builder<History.History>.CreateListOfSize(2)
                                                  .BuildList();

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(history);

            Mocker.GetMock<ITrackedDownloadAlreadyImported>()
                  .Setup(s => s.IsImported(_trackedDownload, history))
                  .Returns(true);

            Subject.Import(_trackedDownload);

            AssertNotImported();
        }

        [Test]
        public void should_not_mark_as_imported_if_some_of_episodes_were_not_imported_including_history()
        {
            var albums = Builder<Book>.CreateListOfSize(3).BuildList();

            _trackedDownload.RemoteAlbum.Albums = albums;

            Mocker.GetMock<IDownloadedTracksImportService>()
                .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Author>(), It.IsAny<DownloadClientItem>()))
                .Returns(new List<ImportResult>
                {
                    new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv" })),
                    new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv" }), "Test Failure"),
                    new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv" }), "Test Failure")
                });

            var history = Builder<History.History>.CreateListOfSize(2)
                                                  .BuildList();

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(history);

            Mocker.GetMock<ITrackedDownloadAlreadyImported>()
                  .Setup(s => s.IsImported(It.IsAny<TrackedDownload>(), It.IsAny<List<History.History>>()))
                  .Returns(false);

            Subject.Import(_trackedDownload);

            AssertNotImported();
        }

        [Test]
        public void should_mark_as_imported_if_all_tracks_were_imported()
        {
            _trackedDownload.RemoteAlbum.Albums = new List<Book>
            {
                CreateAlbum(1)
            };

            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Author>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision<LocalTrack>(
                                       new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic() })),

                               new ImportResult(
                                   new ImportDecision<LocalTrack>(
                                       new LocalTrack { Path = @"C:\TestPath\Droned.S01E02.mkv".AsOsAgnostic() }))
                           });

            Subject.Import(_trackedDownload);

            AssertImported();
        }

        [Test]
        public void should_mark_as_imported_if_all_episodes_were_imported_including_history()
        {
            var albums = Builder<Book>.CreateListOfSize(2).BuildList();

            _trackedDownload.RemoteAlbum.Albums = albums;

            Mocker.GetMock<IDownloadedTracksImportService>()
                .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Author>(), It.IsAny<DownloadClientItem>()))
                .Returns(new List<ImportResult>
                {
                    new ImportResult(
                        new ImportDecision<LocalTrack>(
                            new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv", Album = albums[0] })),

                    new ImportResult(
                        new ImportDecision<LocalTrack>(
                            new LocalTrack { Path = @"C:\TestPath\Droned.S01E02.mkv", Album = albums[1] }), "Test Failure")
                });

            var history = Builder<History.History>.CreateListOfSize(2)
                                                  .BuildList();

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(history);

            Mocker.GetMock<ITrackedDownloadAlreadyImported>()
                  .Setup(s => s.IsImported(It.IsAny<TrackedDownload>(), It.IsAny<List<History.History>>()))
                  .Returns(true);

            Subject.Import(_trackedDownload);

            AssertImported();
        }

        [Test]
        public void should_mark_as_imported_if_the_download_can_be_tracked_using_the_source_seriesid()
        {
            GivenABadlyNamedDownload();

            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Author>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic() }))
                           });

            Subject.Import(_trackedDownload);

            AssertImported();
        }

        private void AssertNotImported()
        {
            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<DownloadCompletedEvent>()), Times.Never());

            _trackedDownload.State.Should().Be(TrackedDownloadState.ImportFailed);
        }

        private void AssertImported()
        {
            Mocker.GetMock<IDownloadedTracksImportService>()
                .Verify(v => v.ProcessPath(_trackedDownload.DownloadItem.OutputPath.FullPath, ImportMode.Auto, _trackedDownload.RemoteAlbum.Artist, _trackedDownload.DownloadItem), Times.Once());

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<DownloadCompletedEvent>()), Times.Once());

            _trackedDownload.State.Should().Be(TrackedDownloadState.Imported);
        }
    }
}
