using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class ReleaseBranchCheckFixture : CoreTest<ReleaseBranchCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Some Warning Message");
        }

        private void GivenValidBranch(string branch)
        {
            Mocker.GetMock<IConfigFileProvider>()
                    .SetupGet(s => s.Branch)
                    .Returns(branch);
        }

        [TestCase("aphrodite")]
        [TestCase("qbit-fix")]
        [TestCase("phantom")]
        public void should_return_warning_when_branch_is_not_valid(string branch)
        {
            GivenValidBranch(branch);

            Subject.Check().ShouldBeWarning();
        }

        [TestCase("nightly")]
        [TestCase("Nightly")]
        [TestCase("develop")]
        [TestCase("master")]
        public void should_return_no_warning_when_branch_valid(string branch)
        {
            GivenValidBranch(branch);

            Subject.Check().ShouldBeOk();
        }
    }
}
