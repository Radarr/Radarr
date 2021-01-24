using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.DiskSpace;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DiskSpace
{
    [TestFixture]
    public class DiskSpaceServiceFixture : CoreTest<DiskSpaceService>
    {
        private RootFolder _rootDir;
        private string _authorFolder1;
        private string _authorFolder2;

        [SetUp]
        public void SetUp()
        {
            _rootDir = new RootFolder { Path = @"G:\fasdlfsdf".AsOsAgnostic() };
            _authorFolder1 = Path.Combine(_rootDir.Path, "author1");
            _authorFolder2 = Path.Combine(_rootDir.Path, "author2");

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

            GivenAuthor();
        }

        private void GivenAuthor(params Author[] author)
        {
            Mocker.GetMock<IAuthorService>()
                  .Setup(v => v.GetAllAuthors())
                  .Returns(author.ToList());
        }

        private void GivenExistingFolder(string folder)
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FolderExists(folder))
                  .Returns(true);
        }

        [Test]
        public void should_check_diskspace_for_author_folders()
        {
            GivenAuthor(new Author { Path = _authorFolder1 });

            GivenExistingFolder(_authorFolder1);

            var freeSpace = Subject.GetFreeSpace();

            freeSpace.Should().NotBeEmpty();
        }

        [Test]
        public void should_check_diskspace_for_same_root_folder_only_once()
        {
            GivenAuthor(new Author { Path = _authorFolder1 }, new Author { Path = _authorFolder2 });

            GivenExistingFolder(_authorFolder1);
            GivenExistingFolder(_authorFolder2);

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
