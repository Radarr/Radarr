using NUnit.Framework;
using NzbDrone.Common.Test.DiskTests;
using NzbDrone.Mono.Disk;

namespace NzbDrone.Mono.Test.DiskProviderTests
{
    [TestFixture]
    [Platform(Exclude = "Win")]
    public class FreeSpaceFixture : FreeSpaceFixtureBase<DiskProvider>
    {
        public FreeSpaceFixture()
        {
            PosixOnly();
        }
    }
}
