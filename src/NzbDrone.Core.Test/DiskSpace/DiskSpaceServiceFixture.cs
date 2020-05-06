using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.DiskSpace;
using NzbDrone.Core.Music;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DiskSpace
{
    [TestFixture]
    public class DiskSpaceServiceFixture : CoreTest<DiskSpaceService>
    {
        private RootFolder _rootDir;
        private string _artistFolder1;
        private string _artistFolder2;

        [SetUp]
        public void SetUp()
        {
            _rootDir = new RootFolder { Path = @"G:\fasdlfsdf".AsOsAgnostic() };
            _artistFolder1 = Path.Combine(_rootDir.Path, "artist1");
            _artistFolder2 = Path.Combine(_rootDir.Path, "artist2");

            Mocker.GetMock<IRootFolderService>()
                  .Setup(x => x.All())
                  .Returns(new List<RootFolder>() { _rootDir });

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

            GivenArtist();
        }

        private void GivenArtist(params Author[] artist)
        {
            Mocker.GetMock<IArtistService>()
                  .Setup(v => v.GetAllArtists())
                  .Returns(artist.ToList());
        }

        private void GivenExistingFolder(string folder)
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FolderExists(folder))
                  .Returns(true);
        }

        [Test]
        public void should_check_diskspace_for_artist_folders()
        {
            GivenArtist(new Author { Path = _artistFolder1 });

            GivenExistingFolder(_artistFolder1);

            var freeSpace = Subject.GetFreeSpace();

            freeSpace.Should().NotBeEmpty();
        }

        [Test]
        public void should_check_diskspace_for_same_root_folder_only_once()
        {
            GivenArtist(new Author { Path = _artistFolder1 }, new Author { Path = _artistFolder2 });

            GivenExistingFolder(_artistFolder1);
            GivenExistingFolder(_artistFolder2);

            var freeSpace = Subject.GetFreeSpace();

            freeSpace.Should().HaveCount(1);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.GetAvailableSpace(It.IsAny<string>()), Times.Once());
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

            Mocker.GetMock<IRootFolderService>()
                  .Setup(x => x.All())
                  .Returns(new List<RootFolder>());

            var freeSpace = Subject.GetFreeSpace();

            freeSpace.Should().BeEmpty();
        }
    }
}
