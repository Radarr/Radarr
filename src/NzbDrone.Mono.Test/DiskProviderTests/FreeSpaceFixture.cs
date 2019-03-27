using NUnit.Framework;
using NzbDrone.Common.Test.DiskTests;
using NzbDrone.Mono.Disk;

namespace NzbDrone.Mono.Test.DiskProviderTests
{
    [TestFixture]
    [Platform("Mono")]
    public class FreeSpaceFixture : FreeSpaceFixtureBase<DiskProvider>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<ISymbolicLinkResolver>(Mocker.Resolve<SymbolicLinkResolver>());
            Mocker.SetConstant<IProcMountProvider>(Mocker.Resolve<ProcMountProvider>());
        }

        public FreeSpaceFixture()
        {
            MonoOnly();
        }
    }
}
