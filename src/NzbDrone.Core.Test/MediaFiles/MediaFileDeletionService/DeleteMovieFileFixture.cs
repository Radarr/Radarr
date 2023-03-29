using System.IO;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaFileDeletionService
{
    [TestFixture]
    public class DeleteMovieFileFixture : CoreTest<Core.MediaFiles.MediaFileDeletionService>
    {
        private const string RootFolder = @"C:\Test\Movies";
        private Movie _movie;
        private MovieFile _movieFile;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .With(s => s.Path = Path.Combine(RootFolder, "Movie Title"))
                                     .Build();

            _movieFile = Builder<MovieFile>.CreateNew()
                                               .With(f => f.RelativePath = "Some SubFolder")
                                               .With(f => f.Path = Path.Combine(_movie.Path, "Some SubFolder"))
                                               .Build();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(_movie.Path))
                  .Returns(RootFolder);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(_movieFile.Path))
                  .Returns(_movie.Path);
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
                  .Returns(new[] { _movie.Path });
        }

        private void GivenMovieFolderExists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(_movie.Path))
                  .Returns(true);
        }

        [Test]
        public void should_throw_if_root_folder_does_not_exist()
        {
            Assert.Throws<NzbDroneClientException>(() => Subject.DeleteMovieFile(_movie, _movieFile));
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_should_throw_if_root_folder_is_empty()
        {
            GivenRootFolderExists();

            Assert.Throws<NzbDroneClientException>(() => Subject.DeleteMovieFile(_movie, _movieFile));
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_delete_from_db_if_movie_folder_does_not_exist()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();

            Subject.DeleteMovieFile(_movie, _movieFile);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_movieFile, DeleteMediaFileReason.Manual), Times.Once());
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_movieFile.Path, It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_delete_from_db_if_movie_file_does_not_exist()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();
            GivenMovieFolderExists();

            Subject.DeleteMovieFile(_movie, _movieFile);

            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_movieFile, DeleteMediaFileReason.Manual), Times.Once());
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_movieFile.Path, It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_delete_from_disk_and_db_if_movie_file_exists()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();
            GivenMovieFolderExists();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(_movieFile.Path))
                  .Returns(true);

            Subject.DeleteMovieFile(_movie, _movieFile);

            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_movieFile.Path, "Movie Title"), Times.Once());
            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_movieFile, DeleteMediaFileReason.Manual), Times.Once());
        }

        [Test]
        public void should_handle_error_deleting_movie_file()
        {
            GivenRootFolderExists();
            GivenRootFolderHasFolders();
            GivenMovieFolderExists();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(_movieFile.Path))
                  .Returns(true);

            Mocker.GetMock<IRecycleBinProvider>()
                  .Setup(s => s.DeleteFile(_movieFile.Path, "Movie Title"))
                  .Throws(new IOException());

            Assert.Throws<NzbDroneClientException>(() => Subject.DeleteMovieFile(_movie, _movieFile));

            ExceptionVerification.ExpectedErrors(1);
            Mocker.GetMock<IRecycleBinProvider>().Verify(v => v.DeleteFile(_movieFile.Path, "Movie Title"), Times.Once());
            Mocker.GetMock<IMediaFileService>().Verify(v => v.Delete(_movieFile, DeleteMediaFileReason.Manual), Times.Never());
        }
    }
}
