using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.TrackImport;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Test.Common;
using NzbDrone.Core.MediaFiles.Events;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class CompletedDownloadServiceFixture : CoreTest<CompletedDownloadService>
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
                    .With(c => c.State = TrackedDownloadStage.Downloading)
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

        private Album CreateAlbum(int id, int trackCount)
        {
            return new Album {
                Id = id,
                AlbumReleases = new List<AlbumRelease> {
                    new AlbumRelease {
                        Monitored = true,
                        TrackCount = trackCount
                    }
                }
            };
        }

        private RemoteAlbum BuildRemoteAlbum()
        {
            return new RemoteAlbum
            {
                Artist = new Artist(),
                Albums = new List<Album> { CreateAlbum(1, 1) }
            };
        }


        private void GivenNoGrabbedHistory()
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.MostRecentForDownloadId(_trackedDownload.DownloadItem.DownloadId))
                .Returns((History.History)null);
        }

        private void GivenSuccessfulImport()
        {
            Mocker.GetMock<IDownloadedTracksImportService>()
                .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Artist>(), It.IsAny<DownloadClientItem>()))
                .Returns(new List<ImportResult>
                    {
                        new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack { Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic() }))
                    });
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
               .Returns((Artist)null);

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

        [TestCase(DownloadItemStatus.Downloading)]
        [TestCase(DownloadItemStatus.Failed)]
        [TestCase(DownloadItemStatus.Queued)]
        [TestCase(DownloadItemStatus.Paused)]
        [TestCase(DownloadItemStatus.Warning)]
        public void should_not_process_if_download_status_isnt_completed(DownloadItemStatus status)
        {
            _trackedDownload.DownloadItem.Status = status;

            Subject.Process(_trackedDownload);

            AssertNoAttemptedImport();
        }

        [Test]
        public void should_not_process_if_matching_history_is_not_found_and_no_category_specified()
        {
            _trackedDownload.DownloadItem.Category = null;
            GivenNoGrabbedHistory();

            Subject.Process(_trackedDownload);

            AssertNoAttemptedImport();
        }

        [Test]
        public void should_process_if_matching_history_is_not_found_but_category_specified()
        {
            _trackedDownload.DownloadItem.Category = "tv";
            GivenNoGrabbedHistory();
            GivenArtistMatch();
            GivenSuccessfulImport();

            Subject.Process(_trackedDownload);

            AssertCompletedDownload();
        }

        [Test]
        public void should_not_process_if_output_path_is_empty()
        {
            _trackedDownload.DownloadItem.OutputPath = new OsPath();

            Subject.Process(_trackedDownload);

            AssertNoAttemptedImport();
        }

        [Test]
        public void should_not_throw_if_remotealbum_is_null()
        {
            _trackedDownload.RemoteAlbum = null;

            Subject.Process(_trackedDownload);

            AssertNoAttemptedImport();
        }

        [Test]
        public void should_mark_as_imported_if_all_tracks_were_imported()
        {
            _trackedDownload.RemoteAlbum.Albums = new List<Album>
            {
                CreateAlbum(1, 2)
            };

            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Artist>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision<LocalTrack>(
                                       new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()})),

                                new ImportResult(
                                   new ImportDecision<LocalTrack>(
                                       new LocalTrack {Path = @"C:\TestPath\Droned.S01E02.mkv".AsOsAgnostic()}))
                           });

            Subject.Process(_trackedDownload);

            AssertCompletedDownload();
        }

        [Test]
        public void should_mark_as_imported_if_all_tracks_were_imported_but_album_incomplete()
        {
            _trackedDownload.RemoteAlbum.Albums = new List<Album>
            {
                CreateAlbum(1, 3)
            };

            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Artist>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision<LocalTrack>(
                                       new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()})),

                                new ImportResult(
                                   new ImportDecision<LocalTrack>(
                                       new LocalTrack {Path = @"C:\TestPath\Droned.S01E02.mkv".AsOsAgnostic()}))
                           });

            Subject.Process(_trackedDownload);

            AssertCompletedDownload();
        }

        [Test]
        public void should_not_mark_as_imported_if_all_files_were_rejected()
        {
            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Artist>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision<LocalTrack>(
                                       new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()}, new Rejection("Rejected!")), "Test Failure"),

                                new ImportResult(
                                   new ImportDecision<LocalTrack>(
                                       new LocalTrack {Path = @"C:\TestPath\Droned.S01E02.mkv".AsOsAgnostic()},new Rejection("Rejected!")), "Test Failure")
                           });

            Subject.Process(_trackedDownload);

            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent<DownloadCompletedEvent>(It.IsAny<DownloadCompletedEvent>()), Times.Never());

            AssertImportIncomplete();
        }

        [Test]
        public void should_not_mark_as_imported_if_no_tracks_were_parsed()
        {
            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Artist>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision<LocalTrack>(
                                       new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()}, new Rejection("Rejected!")), "Test Failure"),

                                new ImportResult(
                                   new ImportDecision<LocalTrack>(
                                       new LocalTrack {Path = @"C:\TestPath\Droned.S01E02.mkv".AsOsAgnostic()},new Rejection("Rejected!")), "Test Failure")
                           });

            _trackedDownload.RemoteAlbum.Albums.Clear();

            Subject.Process(_trackedDownload);

            AssertImportIncomplete();
        }

        [Test]
        public void should_not_mark_as_imported_if_all_files_were_skipped()
        {
            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Artist>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()}),"Test Failure"),
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()}),"Test Failure")
                           });


            Subject.Process(_trackedDownload);

            AssertImportIncomplete();
        }

        [Test]
        public void should_mark_as_imported_if_all_tracks_were_imported_but_extra_files_were_not()
        {
            GivenArtistMatch();

            _trackedDownload.RemoteAlbum.Albums = new List<Album>
            {
                CreateAlbum(1, 3)
            };

            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Artist>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()})),
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()})),
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()})),
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()}), "Test Failure")
                           });

            Subject.Process(_trackedDownload);

            AssertCompletedDownload();
        }

        [Test]
        public void should_mark_as_failed_if_some_tracks_were_not_imported()
        {
            _trackedDownload.RemoteAlbum.Albums = new List<Album>
            {
                CreateAlbum(1, 1),
                CreateAlbum(1, 2),
                CreateAlbum(1, 1)
            };

            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Artist>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()})),
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()})),
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()})),
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()}), "Test Failure"),
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()}), "Test Failure")
                           });


            Subject.Process(_trackedDownload);

            AssertImportIncomplete();
        }

        [Test]
        public void should_mark_as_imported_if_the_download_can_be_tracked_using_the_source_seriesid()
        {
            GivenABadlyNamedDownload();

            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Artist>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()}))
                           });

            Mocker.GetMock<IArtistService>()
                  .Setup(v => v.GetArtist(It.IsAny<int>()))
                  .Returns(BuildRemoteAlbum().Artist);

            Subject.Process(_trackedDownload);

            AssertCompletedDownload();
        }

        [Test]
        public void should_not_mark_as_imported_if_the_download_cannot_be_tracked_using_the_source_title_as_it_was_initiated_externally()
        {
            GivenABadlyNamedDownload();

            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Artist>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()}))
                           });

            Mocker.GetMock<IHistoryService>()
            .Setup(s => s.MostRecentForDownloadId(It.Is<string>(i => i == "1234")));

            Subject.Process(_trackedDownload);

            AssertNoCompletedDownload();
        }

        [Test]
        public void should_not_import_when_there_is_a_title_mismatch()
        {
            _trackedDownload.RemoteAlbum.Artist = null;
            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetArtist("Drone.S01E01.HDTV"))
                  .Returns((Artist)null);

            Subject.Process(_trackedDownload);

            AssertNoCompletedDownload();
        }

        [Test]
        public void should_mark_as_import_title_mismatch_if_ignore_warnings_is_true()
        {
            _trackedDownload.RemoteAlbum.Albums = new List<Album>
            {
                CreateAlbum(0, 1)
            };

            Mocker.GetMock<IDownloadedTracksImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Artist>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack {Path = @"C:\TestPath\Droned.S01E01.mkv".AsOsAgnostic()}))
                           });

            Subject.Process(_trackedDownload, true);

            AssertCompletedDownload();
        }

        [Test]
        public void should_warn_if_path_is_not_valid_for_windows()
        {
            WindowsOnly();

            _trackedDownload.DownloadItem.OutputPath = new OsPath(@"/invalid/Windows/Path");

            Subject.Process(_trackedDownload);

            AssertNoAttemptedImport();
        }

        [Test]
        public void should_warn_if_path_is_not_valid_for_linux()
        {
            MonoOnly();

            _trackedDownload.DownloadItem.OutputPath = new OsPath(@"C:\Invalid\Mono\Path");

            Subject.Process(_trackedDownload);

            AssertNoAttemptedImport();
        }

        private void AssertNoAttemptedImport()
        {
            Mocker.GetMock<IDownloadedTracksImportService>()
                .Verify(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Artist>(), It.IsAny<DownloadClientItem>()), Times.Never());

            AssertNoCompletedDownload();
        }

        private void AssertImportIncomplete()
        {
            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent(It.IsAny<AlbumImportIncompleteEvent>()), Times.Once());

            AssertNoCompletedDownload();
        }

        private void AssertNoCompletedDownload()
        {
            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<DownloadCompletedEvent>()), Times.Never());

            _trackedDownload.State.Should().NotBe(TrackedDownloadStage.Imported);
        }

        private void AssertCompletedDownload()
        {
            Mocker.GetMock<IDownloadedTracksImportService>()
                .Verify(v => v.ProcessPath(_trackedDownload.DownloadItem.OutputPath.FullPath, ImportMode.Auto, _trackedDownload.RemoteAlbum.Artist, _trackedDownload.DownloadItem), Times.Once());

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<DownloadCompletedEvent>()), Times.Once());

            _trackedDownload.State.Should().Be(TrackedDownloadStage.Imported);
        }
    }
}
