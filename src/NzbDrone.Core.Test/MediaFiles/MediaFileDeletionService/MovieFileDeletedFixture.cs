using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaFileDeletionService
{
    [TestFixture]
    public class MovieFileDeletedFixture : CoreTest<Core.MediaFiles.MediaFileDeletionService>
    {
        private Movie _movie;
        private MovieFile _movieFile;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                   .With(s => s.Path = @"C:\Test\Movies\Movie Title".AsOsAgnostic())
                                   .Build();

            _movieFile = Builder<MovieFile>.CreateNew()
                                           .With(f => f.Movie = _movie)
                                           .With(f => f.Path = @"C:\Test\Movies\Movie Title\Movie Title.mkv".AsOsAgnostic())
                                           .Build();

            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DeleteEmptyFolders)
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderEmpty(_movie.Path))
                  .Returns(true);
        }

        [Test]
        public void should_not_delete_movie_folder_if_movie_file_was_missing_from_disk()
        {
            Subject.Handle(new MovieFileDeletedEvent(_movieFile, DeleteMediaFileReason.MissingFromDisk));

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.RemoveEmptySubfolders(It.IsAny<string>()), Times.Never());

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFolder(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_delete_empty_movie_folder_if_movie_file_was_deleted_manually()
        {
            Subject.Handle(new MovieFileDeletedEvent(_movieFile, DeleteMediaFileReason.Manual));

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFolder(_movie.Path, true), Times.Once());
        }

        [Test]
        public void should_not_delete_movie_folder_if_delete_empty_folders_is_disabled()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DeleteEmptyFolders)
                  .Returns(false);

            Subject.Handle(new MovieFileDeletedEvent(_movieFile, DeleteMediaFileReason.Manual));

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFolder(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }
    }
}
