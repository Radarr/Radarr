using NUnit.Framework;
using System.IO.Abstractions.TestingHelpers;
using NzbDrone.Common.Disk;
using Unity.Resolution;

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
