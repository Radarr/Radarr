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
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Test.Common;

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
                                                    .With(h => h.Title = "Drone.1998")
                                                    .Build();

            var remoteEpisode = BuildRemoteMovie();

            _trackedDownload = Builder<TrackedDownload>.CreateNew()
                    .With(c => c.State = TrackedDownloadStage.Downloading)
                    .With(c => c.DownloadItem = completed)
                    .With(c => c.RemoteMovie = remoteEpisode)
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
                  .Setup(s => s.GetMovie("Drone.1998"))
                  .Returns(remoteEpisode.Movie);

        }

        private RemoteMovie BuildRemoteMovie()
        {
			return new RemoteMovie
			{
				Movie = new Movie()
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
            Mocker.GetMock<IDownloadedMovieImportService>()
                .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                .Returns(new List<ImportResult>
                    {
                        new ImportResult(new ImportDecision(new LocalMovie() { Path = @"C:\TestPath\Droned.1998.mkv" }))
                    });
        }


        private void GivenABadlyNamedDownload()
        {
            _trackedDownload.DownloadItem.DownloadId = "1234";
            _trackedDownload.DownloadItem.Title = "Droned Pilot"; // Set a badly named download
            Mocker.GetMock<IHistoryService>()
               .Setup(s => s.MostRecentForDownloadId(It.Is<string>(i => i == "1234")))
               .Returns(new History.History() { SourceTitle = "Droned 1998" });

            Mocker.GetMock<IParsingService>()
               .Setup(s => s.GetMovie(It.IsAny<string>()))
               .Returns((Movie)null);

            Mocker.GetMock<IParsingService>()
                .Setup(s => s.GetMovie("Droned 1998"))
                .Returns(BuildRemoteMovie().Movie);
        }

        private void GivenSeriesMatch()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetMovie(It.IsAny<string>()))
                  .Returns(_trackedDownload.RemoteMovie.Movie);
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
            GivenSeriesMatch();
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
        public void should_mark_as_imported_if_all_episodes_were_imported()
        {
            Mocker.GetMock<IDownloadedMovieImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision(
                                       new LocalMovie {Path = @"C:\TestPath\Droned.1998.mkv"})),

                                new ImportResult(
                                   new ImportDecision(
                                       new LocalMovie {Path = @"C:\TestPath\Droned.1998.mkv"}))
                           });

            Subject.Process(_trackedDownload);

            AssertCompletedDownload();
        }

        [Test]
        public void should_not_mark_as_imported_if_all_files_were_rejected()
        {
            Mocker.GetMock<IDownloadedMovieImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision(
                                       new LocalMovie {Path = @"C:\TestPath\Droned.1998.mkv"}, new Rejection("Rejected!")), "Test Failure"),

                                new ImportResult(
                                   new ImportDecision(
                                       new LocalMovie {Path = @"C:\TestPath\Droned.1999.mkv"},new Rejection("Rejected!")), "Test Failure")
                           });

            Subject.Process(_trackedDownload);

            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent<DownloadCompletedEvent>(It.IsAny<DownloadCompletedEvent>()), Times.Never());

            AssertNoCompletedDownload();
        }

        [Test]
        public void should_not_mark_as_imported_if_no_episodes_were_parsed()
        {
            Mocker.GetMock<IDownloadedMovieImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision(
                                       new LocalMovie {Path = @"C:\TestPath\Droned.1998.mkv"}, new Rejection("Rejected!")), "Test Failure"),

                                new ImportResult(
                                   new ImportDecision(
                                       new LocalMovie {Path = @"C:\TestPath\Droned.1998.mkv"},new Rejection("Rejected!")), "Test Failure")
                           });

			_trackedDownload.RemoteMovie.Movie = null;

            Subject.Process(_trackedDownload);

            AssertNoCompletedDownload();
        }

        [Test]
        public void should_not_mark_as_imported_if_all_files_were_skipped()
        {
            Mocker.GetMock<IDownloadedMovieImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalMovie {Path = @"C:\TestPath\Droned.1998.mkv"}),"Test Failure"),
                               new ImportResult(new ImportDecision(new LocalMovie {Path = @"C:\TestPath\Droned.1998.mkv"}),"Test Failure")
                           });


            Subject.Process(_trackedDownload);

            AssertNoCompletedDownload();
        }

        [Test]
        public void should_mark_as_imported_if_all_episodes_were_imported_but_extra_files_were_not()
        {
            GivenSeriesMatch();

            Mocker.GetMock<IDownloadedMovieImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalMovie {Path = @"C:\TestPath\Droned.1998.mkv"})),
                               new ImportResult(new ImportDecision(new LocalMovie{Path = @"C:\TestPath\Droned.1998.mkv"}),"Test Failure")
                           });

            Subject.Process(_trackedDownload);

            AssertCompletedDownload();
        }

        [Test]
        public void should_mark_as_imported_if_the_download_can_be_tracked_using_the_source_seriesid()
        {
            GivenABadlyNamedDownload();

            Mocker.GetMock<IDownloadedMovieImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalMovie {Path = @"C:\TestPath\Droned.1998.mkv"}))
                           });

            Mocker.GetMock<IMovieService>()
                  .Setup(v => v.GetMovie(It.IsAny<int>()))
                  .Returns(BuildRemoteMovie().Movie);

            Subject.Process(_trackedDownload);

            AssertCompletedDownload();
        }

        [Test]
        public void should_not_mark_as_imported_if_the_download_cannot_be_tracked_using_the_source_title_as_it_was_initiated_externally()
        {
            GivenABadlyNamedDownload();

            Mocker.GetMock<IDownloadedMovieImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalMovie {Path = @"C:\TestPath\Droned.1998.mkv"}))
                           });

            Mocker.GetMock<IHistoryService>()
            .Setup(s => s.MostRecentForDownloadId(It.Is<string>(i => i == "1234")));

            Subject.Process(_trackedDownload);

            AssertNoCompletedDownload();
        }

        [Test]
        public void should_not_import_when_there_is_a_title_mismatch()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetMovie("Drone.1998"))
                  .Returns((Movie)null);

            Subject.Process(_trackedDownload);

            AssertNoCompletedDownload();
        }

        [Test]
        public void should_mark_as_import_title_mismatch_if_ignore_warnings_is_true()
		{

            Mocker.GetMock<IDownloadedMovieImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalMovie {Path = @"C:\TestPath\Droned.1998.mkv"}))
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
            Mocker.GetMock<IDownloadedMovieImportService>()
                .Verify(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()), Times.Never());

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
            Mocker.GetMock<IDownloadedMovieImportService>()
                .Verify(v => v.ProcessPath(_trackedDownload.DownloadItem.OutputPath.FullPath, ImportMode.Auto, _trackedDownload.RemoteMovie.Movie, _trackedDownload.DownloadItem), Times.Once());

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<DownloadCompletedEvent>()), Times.Once());

            _trackedDownload.State.Should().Be(TrackedDownloadStage.Imported);
        }
    }
}
