using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles
{
    public class MediaFileTableCleanupServiceFixture : CoreTest<MediaFileTableCleanupService>
    {
        private const string DELETED_PATH = "ANY FILE WITH THIS PATH IS CONSIDERED DELETED!";
        private Movie _movie;

        [SetUp]
        public void SetUp()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\TV\Series".AsOsAgnostic())
                                     .Build();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(e => e.FileExists(It.Is<string>(c => !c.Contains(DELETED_PATH))))
                  .Returns(true);
        }

        private void GivenMovieFiles(IEnumerable<MovieFile> movieFiles)
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(c => c.GetFilesByMovie(It.IsAny<int>()))
                  .Returns(movieFiles.ToList());
        }

        private List<string> FilesOnDisk(IEnumerable<MovieFile> movieFiles)
        {
            return movieFiles.Select(e => Path.Combine(_movie.Path, e.RelativePath)).ToList();
        }

        [Test]
        public void should_skip_files_that_exist_in_disk()
        {
            var movieFiles = Builder<MovieFile>.CreateListOfSize(10)
                .Build();

            GivenMovieFiles(movieFiles);

            Subject.Clean(_movie, FilesOnDisk(movieFiles));

            Mocker.GetMock<IMovieService>().Verify(c => c.UpdateMovie(It.IsAny<Movie>()), Times.Never());
        }

        [Test]
        public void should_delete_non_existent_files()
        {
            var movieFiles = Builder<MovieFile>.CreateListOfSize(10)
                .Random(2)
                .With(c => c.RelativePath = DELETED_PATH)
                .Build();

            GivenMovieFiles(movieFiles);

            Subject.Clean(_movie, FilesOnDisk(movieFiles.Where(e => e.RelativePath != DELETED_PATH)));

            Mocker.GetMock<IMediaFileService>().Verify(c => c.Delete(It.Is<MovieFile>(e => e.RelativePath == DELETED_PATH), DeleteMediaFileReason.MissingFromDisk), Times.Exactly(2));
        }

        [Test]
        [Ignore("idc")]
        public void should_delete_files_that_dont_belong_to_any_episodes()
        {
            var movieFiles = Builder<MovieFile>.CreateListOfSize(10)
                                .Random(10)
                                .With(c => c.RelativePath = "ExistingPath")
                                .Build();

            GivenMovieFiles(movieFiles);

            Subject.Clean(_movie, FilesOnDisk(movieFiles));

            Mocker.GetMock<IMediaFileService>().Verify(c => c.Delete(It.IsAny<MovieFile>(), DeleteMediaFileReason.NoLinkedEpisodes), Times.Exactly(10));
        }

        [Test]
        public void should_not_update_episode_when_episodeFile_exists()
        {
            var movieFiles = Builder<MovieFile>.CreateListOfSize(10)
                                .Random(10)
                                .With(c => c.RelativePath = "ExistingPath")
                                .Build();

            GivenMovieFiles(movieFiles);

            Subject.Clean(_movie, FilesOnDisk(movieFiles));

            Mocker.GetMock<IMovieService>().Verify(c => c.UpdateMovie(It.IsAny<Movie>()), Times.Never());
        }
    }
}
