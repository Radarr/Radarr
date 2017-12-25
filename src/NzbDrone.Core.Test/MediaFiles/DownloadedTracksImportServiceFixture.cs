using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.TrackImport;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class DownloadedTracksImportServiceFixture : CoreTest<DownloadedTracksImportService>
    {
        private string _droneFactory = "c:\\drop\\".AsOsAgnostic();
        private string[] _subFolders = new[] { "c:\\root\\foldername".AsOsAgnostic() };
        private string[] _audioFiles = new[] { "c:\\root\\foldername\\01 the first track.ext".AsOsAgnostic() };

        private TrackedDownload _trackedDownload;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IDiskScanService>().Setup(c => c.GetAudioFiles(It.IsAny<string>(), It.IsAny<bool>()))
                  .Returns(_audioFiles);

            Mocker.GetMock<IDiskScanService>().Setup(c => c.FilterFiles(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                  .Returns<string, IEnumerable<string>>((b, s) => s.ToList());

            Mocker.GetMock<IDiskProvider>().Setup(c => c.GetDirectories(It.IsAny<string>()))
                  .Returns(_subFolders);

            Mocker.GetMock<IDiskProvider>().Setup(c => c.FolderExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IImportApprovedTracks>()
                  .Setup(s => s.Import(It.IsAny<List<ImportDecision>>(), true, null, ImportMode.Auto))
                  .Returns(new List<ImportResult>());

            var downloadItem = Builder<DownloadClientItem>.CreateNew()
                .With(v => v.DownloadId = "sab1")
                .With(v => v.Status = DownloadItemStatus.Downloading)
                .Build();

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                .With(v => v.Artist = new Artist())
                .Build();

            _trackedDownload = new TrackedDownload
           
            {
                DownloadItem = downloadItem,
                RemoteAlbum = remoteAlbum,
                State = TrackedDownloadStage.Downloading
             };
        }

        private void GivenValidArtist()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetArtist(It.IsAny<string>()))
                  .Returns(Builder<Artist>.CreateNew().Build());
        }

        private void GivenSuccessfulImport()
        {
            var localTrack = new LocalTrack();

            var imported = new List<ImportDecision>();
            imported.Add(new ImportDecision(localTrack));

            Mocker.GetMock<IMakeImportDecision>()
                  .Setup(s => s.GetImportDecisions(It.IsAny<List<string>>(), It.IsAny<Artist>(), null))
                  .Returns(imported);

            Mocker.GetMock<IImportApprovedTracks>()
                  .Setup(s => s.Import(It.IsAny<List<ImportDecision>>(), It.IsAny<bool>(), It.IsAny<DownloadClientItem>(), It.IsAny<ImportMode>()))
                  .Returns(imported.Select(i => new ImportResult(i)).ToList())
                  .Callback(() => WasImportedResponse());
        }

        private void WasImportedResponse()
        {
            Mocker.GetMock<IDiskScanService>().Setup(c => c.GetAudioFiles(It.IsAny<string>(), It.IsAny<bool>()))
                  .Returns(new string[0]);
        }

        [Test]
        public void should_search_for_artist_using_folder_name()
        {
            Subject.ProcessRootFolder(new DirectoryInfo(_droneFactory));

            Mocker.GetMock<IParsingService>().Verify(c => c.GetArtist("foldername"), Times.Once());
        }

        [Test]
        public void should_skip_if_file_is_in_use_by_another_process()
        {
            GivenValidArtist();

            Mocker.GetMock<IDiskProvider>().Setup(c => c.IsFileLocked(It.IsAny<string>()))
                  .Returns(true);

            Subject.ProcessRootFolder(new DirectoryInfo(_droneFactory));

            VerifyNoImport();
        }

        [Test]
        public void should_skip_if_no_artist_found()
        {
            Mocker.GetMock<IParsingService>().Setup(c => c.GetArtist("foldername")).Returns((Artist)null);

            Subject.ProcessRootFolder(new DirectoryInfo(_droneFactory));

            Mocker.GetMock<IMakeImportDecision>()
                .Verify(c => c.GetImportDecisions(It.IsAny<List<string>>(), It.IsAny<Artist>(), It.IsAny<ParsedTrackInfo>()),
                    Times.Never());

            VerifyNoImport();
        }

        [Test]
        public void should_not_import_if_folder_is_a_artist_path()
        {
            GivenValidArtist();

            Mocker.GetMock<IArtistService>()
                  .Setup(s => s.ArtistPathExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskScanService>()
                  .Setup(c => c.GetAudioFiles(It.IsAny<string>(), It.IsAny<bool>()))
                  .Returns(new string[0]);

            Subject.ProcessRootFolder(new DirectoryInfo(_droneFactory));

            Mocker.GetMock<IDiskScanService>()
                  .Verify(v => v.GetAudioFiles(It.IsAny<string>(), true), Times.Never());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_not_delete_folder_if_no_files_were_imported()
        {
            Mocker.GetMock<IImportApprovedTracks>()
                  .Setup(s => s.Import(It.IsAny<List<ImportDecision>>(), false, null, ImportMode.Auto))
                  .Returns(new List<ImportResult>());

            Subject.ProcessRootFolder(new DirectoryInfo(_droneFactory));

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.GetFolderSize(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_not_delete_folder_if_files_were_imported_and_audio_files_remain()
        {
            GivenValidArtist();

            var localTrack = new LocalTrack();

            var imported = new List<ImportDecision>();
            imported.Add(new ImportDecision(localTrack));

            Mocker.GetMock<IMakeImportDecision>()
                  .Setup(s => s.GetImportDecisions(It.IsAny<List<string>>(), It.IsAny<Artist>(), null))
                  .Returns(imported);

            Mocker.GetMock<IImportApprovedTracks>()
                  .Setup(s => s.Import(It.IsAny<List<ImportDecision>>(), true, null, ImportMode.Auto))
                  .Returns(imported.Select(i => new ImportResult(i)).ToList());

            Subject.ProcessRootFolder(new DirectoryInfo(_droneFactory));

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFolder(It.IsAny<string>(), true), Times.Never());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_delete_folder_if_files_were_imported_and_only_sample_files_remain()
        {
            GivenValidArtist();

            var localTrack = new LocalTrack();

            var imported = new List<ImportDecision>();
            imported.Add(new ImportDecision(localTrack));

            Mocker.GetMock<IMakeImportDecision>()
                  .Setup(s => s.GetImportDecisions(It.IsAny<List<string>>(), It.IsAny<Artist>(), null))
                  .Returns(imported);

            Mocker.GetMock<IImportApprovedTracks>()
                  .Setup(s => s.Import(It.IsAny<List<ImportDecision>>(), true, null, ImportMode.Auto))
                  .Returns(imported.Select(i => new ImportResult(i)).ToList());

            //Mocker.GetMock<IDetectSample>()
            //      .Setup(s => s.IsSample(It.IsAny<Artist>(),
            //          It.IsAny<QualityModel>(),
            //          It.IsAny<string>(),
            //          It.IsAny<long>(),
            //          It.IsAny<bool>()))
            //      .Returns(true);

            Subject.ProcessRootFolder(new DirectoryInfo(_droneFactory));

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFolder(It.IsAny<string>(), true), Times.Once());
        }

        [TestCase("_UNPACK_")]
        [TestCase("_FAILED_")]
        public void should_remove_unpack_from_folder_name(string prefix)
        {
            var folderName = "Alien Ant Farm - Truant (2003)";
            var folders = new[] { string.Format(@"C:\Test\Unsorted\{0}{1}", prefix, folderName).AsOsAgnostic() };

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.GetDirectories(It.IsAny<string>()))
                  .Returns(folders);

            Subject.ProcessRootFolder(new DirectoryInfo(_droneFactory));

            Mocker.GetMock<IParsingService>()
                .Verify(v => v.GetArtist(folderName), Times.Once());

            Mocker.GetMock<IParsingService>()
                .Verify(v => v.GetArtist(It.Is<string>(s => s.StartsWith(prefix))), Times.Never());
        }

        [Test]
        public void should_return_importresult_on_unknown_artist()
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FolderExists(It.IsAny<string>()))
                  .Returns(false);

            Mocker.GetMock<IDiskProvider>().Setup(c => c.FileExists(It.IsAny<string>()))
                  .Returns(true);

            var fileName = @"C:\folder\file.mkv".AsOsAgnostic();

            var result = Subject.ProcessPath(fileName);

            result.Should().HaveCount(1);
            result.First().ImportDecision.Should().NotBeNull();
            result.First().ImportDecision.LocalTrack.Should().NotBeNull();
            result.First().ImportDecision.LocalTrack.Path.Should().Be(fileName);
            result.First().Result.Should().Be(ImportResultType.Rejected);
        }

        [Test]
        public void should_not_delete_if_there_is_large_rar_file()
        {
            GivenValidArtist();

            var localTrack = new LocalTrack();

            var imported = new List<ImportDecision>();
            imported.Add(new ImportDecision(localTrack));

            Mocker.GetMock<IMakeImportDecision>()
                  .Setup(s => s.GetImportDecisions(It.IsAny<List<string>>(), It.IsAny<Artist>(), null))
                  .Returns(imported);

            Mocker.GetMock<IImportApprovedTracks>()
                  .Setup(s => s.Import(It.IsAny<List<ImportDecision>>(), true, null, ImportMode.Auto))
                  .Returns(imported.Select(i => new ImportResult(i)).ToList());

            //Mocker.GetMock<IDetectSample>()
            //      .Setup(s => s.IsSample(It.IsAny<Artist>(),
            //          It.IsAny<QualityModel>(),
            //          It.IsAny<string>(),
            //          It.IsAny<long>(),
            //          It.IsAny<bool>()))
            //      .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFiles(It.IsAny<string>(), SearchOption.AllDirectories))
                  .Returns(new []{ _audioFiles.First().Replace(".ext", ".rar") });

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFileSize(It.IsAny<string>()))
                  .Returns(15.Megabytes());

            Subject.ProcessRootFolder(new DirectoryInfo(_droneFactory));

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFolder(It.IsAny<string>(), true), Times.Never());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_use_folder_if_folder_import()
        {
            GivenValidArtist();

            var folderName = @"C:\media\ba09030e-1234-1234-1234-123456789abc\[HorribleSubs] Maria the Virgin Witch - 09 [720p]".AsOsAgnostic();
            var fileName = @"C:\media\ba09030e-1234-1234-1234-123456789abc\[HorribleSubs] Maria the Virgin Witch - 09 [720p]\[HorribleSubs] Maria the Virgin Witch - 09 [720p].mkv".AsOsAgnostic();

            Mocker.GetMock<IDiskProvider>().Setup(c => c.FolderExists(folderName))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>().Setup(c => c.GetFiles(folderName, SearchOption.TopDirectoryOnly))
                  .Returns(new[] { fileName });

            var localTrack = new LocalTrack();

            var imported = new List<ImportDecision>();
            imported.Add(new ImportDecision(localTrack));


            Subject.ProcessPath(fileName);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(s => s.GetImportDecisions(It.IsAny<List<string>>(), It.IsAny<Artist>(), It.Is<ParsedTrackInfo>(v => v.TrackNumbers.First() == 9)), Times.Once());
        }

        [Test]
        public void should_not_use_folder_if_file_import()
        {
            GivenValidArtist();

            var fileName = @"C:\media\ba09030e-1234-1234-1234-123456789abc\Torrents\[HorribleSubs] Maria the Virgin Witch - 09 [720p].mkv".AsOsAgnostic();

            Mocker.GetMock<IDiskProvider>().Setup(c => c.FolderExists(fileName))
                  .Returns(false);

            Mocker.GetMock<IDiskProvider>().Setup(c => c.FileExists(fileName))
                  .Returns(true);

            var localTrack = new LocalTrack();

            var imported = new List<ImportDecision>();
            imported.Add(new ImportDecision(localTrack));

            var result = Subject.ProcessPath(fileName);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(s => s.GetImportDecisions(It.IsAny<List<string>>(), It.IsAny<Artist>(), null), Times.Once());
        }

        [Test]
        public void should_not_process_if_file_and_folder_do_not_exist()
        {
            var folderName = @"C:\media\ba09030e-1234-1234-1234-123456789abc\[HorribleSubs] Maria the Virgin Witch - 09 [720p]".AsOsAgnostic();

            Mocker.GetMock<IDiskProvider>().Setup(c => c.FolderExists(folderName))
                  .Returns(false);

            Mocker.GetMock<IDiskProvider>().Setup(c => c.FileExists(folderName))
                  .Returns(false);

            Subject.ProcessPath(folderName).Should().BeEmpty();

            Mocker.GetMock<IParsingService>()
                .Verify(v => v.GetArtist(It.IsAny<string>()), Times.Never());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_not_delete_if_no_files_were_imported()
        {
            GivenValidArtist();

            var localTrack = new LocalTrack();

            var imported = new List<ImportDecision>();
            imported.Add(new ImportDecision(localTrack));

            Mocker.GetMock<IMakeImportDecision>()
                  .Setup(s => s.GetImportDecisions(It.IsAny<List<string>>(), It.IsAny<Artist>(), null))
                  .Returns(imported);

            Mocker.GetMock<IImportApprovedTracks>()
                  .Setup(s => s.Import(It.IsAny<List<ImportDecision>>(), true, null, ImportMode.Auto))
                  .Returns(new List<ImportResult>());

            //Mocker.GetMock<IDetectSample>()
            //      .Setup(s => s.IsSample(It.IsAny<Artist>(),
            //          It.IsAny<QualityModel>(),
            //          It.IsAny<string>(),
            //          It.IsAny<long>(),
            //          It.IsAny<bool>()))
            //      .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFileSize(It.IsAny<string>()))
                  .Returns(15.Megabytes());

            Subject.ProcessRootFolder(new DirectoryInfo(_droneFactory));

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFolder(It.IsAny<string>(), true), Times.Never());
        }

        [Test]
        public void should_not_delete_folder_after_import()
        {
            GivenValidArtist();

            GivenSuccessfulImport();

            _trackedDownload.DownloadItem.CanMoveFiles = false;

            Subject.ProcessPath(_droneFactory, ImportMode.Auto, _trackedDownload.RemoteAlbum.Artist, _trackedDownload.DownloadItem);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFolder(It.IsAny<string>(), true), Times.Never());
        }

        [Test]
        public void should_delete_folder_if_importmode_move()
        {
            GivenValidArtist();

            GivenSuccessfulImport();

            _trackedDownload.DownloadItem.CanMoveFiles = false;

            Subject.ProcessPath(_droneFactory, ImportMode.Move, _trackedDownload.RemoteAlbum.Artist, _trackedDownload.DownloadItem);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFolder(It.IsAny<string>(), true), Times.Once());
        }

        [Test]
        public void should_not_delete_folder_if_importmode_copy()
        {
            GivenValidArtist();

            GivenSuccessfulImport();

            _trackedDownload.DownloadItem.CanMoveFiles = true;

            Subject.ProcessPath(_droneFactory, ImportMode.Copy, _trackedDownload.RemoteAlbum.Artist, _trackedDownload.DownloadItem);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFolder(It.IsAny<string>(), true), Times.Never());
        }

        private void VerifyNoImport()
        {
            Mocker.GetMock<IImportApprovedTracks>().Verify(c => c.Import(It.IsAny<List<ImportDecision>>(), true, null, ImportMode.Auto),
                Times.Never());
        }

        private void VerifyImport()
        {
            Mocker.GetMock<IImportApprovedTracks>().Verify(c => c.Import(It.IsAny<List<ImportDecision>>(), true, null, ImportMode.Auto),
                Times.Once());
        }
    }
}
