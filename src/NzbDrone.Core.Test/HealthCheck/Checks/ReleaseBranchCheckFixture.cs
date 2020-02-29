using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class ReleaseBranchCheckFixture : CoreTest<ReleaseBranchCheck>
    {
        private void GivenValidBranch(string branch)
        {
            Mocker.GetMock<IConfigFileProvider>()
                    .SetupGet(s => s.Branch)
                    .Returns(branch);
        }

        [Test]
        public void should_return_warning_when_branch_not_valid()
        {
            GivenValidBranch("master");

            Subject.Check().ShouldBeWarning();
        }

        [TestCase("develop")]
        [TestCase("nightly")]
        [TestCase("aphrodite")]
        [TestCase("Aphrodite")]
        public void should_return_no_warning_when_branch_valid(string branch)
        {
            GivenValidBranch(branch);

            Subject.Check().ShouldBeOk();
        }
    }
}
