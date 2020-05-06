using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.RootFolderTests
{
    [TestFixture]
    public class GetBestRootFolderPathFixture : CoreTest<RootFolderService>
    {
        private void GivenRootFolders(params string[] paths)
        {
            Mocker.GetMock<IRootFolderRepository>()
                .Setup(s => s.All())
                .Returns(paths.Select(p => new RootFolder { Path = p }));
        }

        [Test]
        public void should_return_root_folder_that_is_parent_path()
        {
            GivenRootFolders(@"C:\Test\Movies".AsOsAgnostic(), @"D:\Test\Movies".AsOsAgnostic());
            Subject.GetBestRootFolderPath(@"C:\Test\Movies\Movie Title".AsOsAgnostic()).Should().Be(@"C:\Test\Movies".AsOsAgnostic());
        }

        [Test]
        public void should_return_root_folder_that_is_grandparent_path()
        {
            GivenRootFolders(@"C:\Test\Movies".AsOsAgnostic(), @"D:\Test\Movies".AsOsAgnostic());
            Subject.GetBestRootFolderPath(@"C:\Test\Movies\M\Movie Title".AsOsAgnostic()).Should().Be(@"C:\Test\Movies".AsOsAgnostic());
        }

        [Test]
        public void should_get_parent_path_from_diskProvider_if_matching_root_folder_is_not_found()
        {
            var moviePath = @"T:\Test\Movies\Movie Title".AsOsAgnostic();

            GivenRootFolders(@"C:\Test\Movies".AsOsAgnostic(), @"D:\Test\Movies".AsOsAgnostic());
            Subject.GetBestRootFolderPath(moviePath);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.GetParentFolder(moviePath), Times.Once);
        }
    }
}
