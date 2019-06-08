using NUnit.Framework;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Practices.Unity;
using NzbDrone.Common.Disk;
namespace NzbDrone.Core.Test.Framework
{
    public abstract class FileSystemTest<TSubject> : CoreTest<TSubject> where TSubject : class
    {
        protected MockFileSystem FileSystem { get; private set; }
        protected IDiskProvider DiskProvider { get; private set; }

        [SetUp]
        public void FileSystemTestSetup()
        {
            FileSystem = new MockFileSystem();

            DiskProvider = Mocker.Resolve<IDiskProvider>("ActualDiskProvider", new ResolverOverride[] {
                    new ParameterOverride("fileSystem", FileSystem)
                });
        }
    }
}
