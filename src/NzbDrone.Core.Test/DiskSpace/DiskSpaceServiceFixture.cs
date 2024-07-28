using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.DiskSpace;
using NzbDrone.Core.Movies;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DiskSpace
{
    [TestFixture]
    public class DiskSpaceServiceFixture : CoreTest<DiskSpaceService>
    {
        private string _moviesFolder;
        private string _moviesFolder2;
        private string _rootFolder;

        [SetUp]
        public void SetUp()
        {
            _moviesFolder = @"G:\fasdlfsdf\movies".AsOsAgnostic();
            _moviesFolder2 = @"G:\fasdlfsdf\movies2".AsOsAgnostic();
            _rootFolder = @"G:\fasdlfsdf".AsOsAgnostic();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.GetMounts())
                  .Returns(new List<IMount>());

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.GetPathRoot(It.IsAny<string>()))
                  .Returns(@"G:\".AsOsAgnostic());

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.GetAvailableSpace(It.IsAny<string>()))
                  .Returns(0);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.GetTotalSize(It.IsAny<string>()))
                  .Returns(0);

            GivenMovies();
        }

        private void GivenMovies(params Movie[] movies)
        {
            Mocker.GetMock<IMovieService>()
                .Setup(v => v.AllMoviePaths())
                .Returns(movies.ToDictionary(x => x.Id, x => x.Path));
        }

        private void GivenRootFolder(string moviePath, string rootFolderPath)
        {
            Mocker.GetMock<IRootFolderService>()
                .Setup(v => v.GetBestRootFolderPath(moviePath, null))
                .Returns(rootFolderPath);
        }

        private void GivenExistingFolder(string folder)
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FolderExists(folder))
                  .Returns(true);
        }

        [Test]
        public void should_check_diskspace_for_movies_folders()
        {
            GivenMovies(new Movie { Path = _moviesFolder });
            GivenRootFolder(_moviesFolder, _rootFolder);
            GivenExistingFolder(_rootFolder);

            var freeSpace = Subject.GetFreeSpace();

            freeSpace.Should().NotBeEmpty();
        }

        [Test]
        public void should_check_diskspace_for_same_root_folder_only_once()
        {
            GivenMovies(new Movie { Id = 1, Path = _moviesFolder }, new Movie { Id = 2, Path = _moviesFolder2 });
            GivenRootFolder(_moviesFolder, _rootFolder);
            GivenRootFolder(_moviesFolder, _rootFolder);
            GivenExistingFolder(_rootFolder);

            var freeSpace = Subject.GetFreeSpace();

            freeSpace.Should().HaveCount(1);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.GetAvailableSpace(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_not_check_diskspace_for_missing_movie_root_folders()
        {
            GivenMovies(new Movie { Path = _moviesFolder });
            GivenRootFolder(_moviesFolder, _rootFolder);

            var freeSpace = Subject.GetFreeSpace();

            freeSpace.Should().BeEmpty();

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.GetAvailableSpace(It.IsAny<string>()), Times.Never());
        }

        [TestCase("/boot")]
        [TestCase("/var/lib/rancher")]
        [TestCase("/var/lib/rancher/volumes")]
        [TestCase("/var/lib/kubelet")]
        [TestCase("/var/lib/docker")]
        [TestCase("/some/place/docker/aufs")]
        [TestCase("/etc/network")]
        public void should_not_check_diskspace_for_irrelevant_mounts(string path)
        {
            var mount = new Mock<IMount>();
            mount.SetupGet(v => v.RootDirectory).Returns(path);
            mount.SetupGet(v => v.DriveType).Returns(System.IO.DriveType.Fixed);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.GetMounts())
                  .Returns(new List<IMount> { mount.Object });

            var freeSpace = Subject.GetFreeSpace();

            freeSpace.Should().BeEmpty();
        }
    }
}
