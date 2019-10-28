using Moq;
using NzbDrone.Common.Test.DiskTests;
using NzbDrone.Mono.Disk;
using NUnit.Framework;
using FluentAssertions;

namespace NzbDrone.Mono.Test.DiskProviderTests
{
    [TestFixture]
    [Platform(Exclude="Win")]
    public class FreeSpaceFixture : FreeSpaceFixtureBase<DiskProvider>
    {
        public FreeSpaceFixture()
        {
            MonoOnly();
        }

        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<IProcMountProvider>(new ProcMountProvider(TestLogger));
            Mocker.GetMock<ISymbolicLinkResolver>()
                  .Setup(v => v.GetCompleteRealPath(It.IsAny<string>()))
                  .Returns<string>(s => s);
        }

        [Test]
        public void should_be_able_to_check_space_on_ramdrive()
        {
            Subject.GetAvailableSpace("/run/").Should().NotBe(0);
        }
    }
}
