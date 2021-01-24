using System.IO;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaFileDeletionService
{
    [TestFixture]
    public class DeleteTrackFileFixture : CoreTest<Core.MediaFiles.MediaFileDeletionService>
    {
        private static readonly string RootFolder = @"C:\Test\Music";
        private Author _author;
        private BookFile _trackFile;

        [SetUp]
        public void Setup()
        {
            _author = Builder<Author>.CreateNew()
                                     .With(s => s.Path = Path.Combine(RootFolder, "Author Name"))
                                     .Build();

            _trackFile = Builder<BookFile>.CreateNew()
                                               .With(f => f.Path = "/Author Name - Track01")
                                               .Build();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(_author.Path))
                  .Returns(RootFolder);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(_trackFile.Path))
                  .Returns(_author.Path);
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
                  .Returns(new[] { _author.Path });
        }

        private void GivenAuthorFolderExists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(_author.Path))
                  .Returns(true);
        }

        [Test]
        public void should_throw_if_root_folder_does_not_exist()
        {
            Assert.Throws<NzbDroneClientException>(() => Subject.DeleteTrackFile(_author, _trackFile));
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_throw_if_root_folder_is_empty()
        {
            GivenRootFolderExists();
            Assert.Throws<NzbDroneClientException>(() => Subject.DeleteTrackFile(_author, _trackFile));
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_delete_from_db_if_author_folder_does_not_exist()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();

            Subject.DeleteTrackFile(_author, _trackFile);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_trackFile, DeleteMediaFileReason.Manual), Times.Once());
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_trackFile.Path, It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_delete_from_db_if_track_file_does_not_exist()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();
            GivenAuthorFolderExists();

            Subject.DeleteTrackFile(_author, _trackFile);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_trackFile, DeleteMediaFileReason.Manual), Times.Once());
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_trackFile.Path, It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_delete_from_disk_and_db_if_track_file_exists()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();
            GivenAuthorFolderExists();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(_trackFile.Path))
                  .Returns(true);

            Subject.DeleteTrackFile(_author, _trackFile);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_trackFile.Path, "Author Name"), Times.Once());
            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_trackFile, DeleteMediaFileReason.Manual), Times.Once());
        }

        [Test]
        public void should_handle_error_deleting_track_file()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();
            GivenAuthorFolderExists();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(_trackFile.Path))
                  .Returns(true);

            Mocker.GetMock<IRecycleBinProvider>()
                  .Setup(s => s.DeleteFile(_trackFile.Path, "Author Name"))
                  .Throws(new IOException());

            Assert.Throws<NzbDroneClientException>(() => Subject.DeleteTrackFile(_author, _trackFile));

            ExceptionVerification.ExpectedErrors(1);
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_trackFile.Path, "Author Name"), Times.Once());
            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_trackFile, DeleteMediaFileReason.Manual), Times.Never());
        }
    }
}
