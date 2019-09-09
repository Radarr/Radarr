using System.IO;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaFileDeletionService
{
    [TestFixture]
    public class DeleteTrackFileFixture : CoreTest<Core.MediaFiles.MediaFileDeletionService>
    {
        private static readonly string RootFolder = @"C:\Test\Music";
        private Artist _artist;
        private TrackFile _trackFile;

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>.CreateNew()
                                     .With(s => s.Path = Path.Combine(RootFolder, "Artist Name"))
                                     .Build();

            _trackFile = Builder<TrackFile>.CreateNew()
                                               .With(f => f.Path = "/Artist Name - Track01")
                                               .Build();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(_artist.Path))
                  .Returns(RootFolder);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(_trackFile.Path))
                  .Returns(_artist.Path);
        }

        private void GivenRootFolderExists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(RootFolder))
                  .Returns(true);
        }

        private void GivenRootFolderHasFolders()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetDirectories(RootFolder))
                  .Returns(new[] { _artist.Path });
        }

        private void GivenArtistFolderExists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(_artist.Path))
                  .Returns(true);
        }

        [Test]
        public void should_throw_if_root_folder_does_not_exist()
        {
            Assert.Throws<NzbDroneClientException>(() => Subject.DeleteTrackFile(_artist, _trackFile));
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_throw_if_root_folder_is_empty()
        {
            GivenRootFolderExists();
            Assert.Throws<NzbDroneClientException>(() => Subject.DeleteTrackFile(_artist, _trackFile));
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_delete_from_db_if_artist_folder_does_not_exist()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();

            Subject.DeleteTrackFile(_artist, _trackFile);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_trackFile, DeleteMediaFileReason.Manual), Times.Once());
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_trackFile.Path, It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_delete_from_db_if_track_file_does_not_exist()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();
            GivenArtistFolderExists();

            Subject.DeleteTrackFile(_artist, _trackFile);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_trackFile, DeleteMediaFileReason.Manual), Times.Once());
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_trackFile.Path, It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_delete_from_disk_and_db_if_track_file_exists()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();
            GivenArtistFolderExists();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(_trackFile.Path))
                  .Returns(true);

            Subject.DeleteTrackFile(_artist, _trackFile);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_trackFile.Path, "Artist Name"), Times.Once());
            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_trackFile, DeleteMediaFileReason.Manual), Times.Once());
        }

        [Test]
        public void should_handle_error_deleting_track_file()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();
            GivenArtistFolderExists();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(_trackFile.Path))
                  .Returns(true);

            Mocker.GetMock<IRecycleBinProvider>()
                  .Setup(s => s.DeleteFile(_trackFile.Path, "Artist Name"))
                  .Throws(new IOException());

            Assert.Throws<NzbDroneClientException>(() => Subject.DeleteTrackFile(_artist, _trackFile));

            ExceptionVerification.ExpectedErrors(1);
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_trackFile.Path, "Artist Name"), Times.Once());
            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_trackFile, DeleteMediaFileReason.Manual), Times.Never());
        }
    }
}
