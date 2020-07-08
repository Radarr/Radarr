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

        [Test]
        public void should_return_warning_when_branch_not_valid()
        {
            GivenValidBranch("master");

            Subject.Check().ShouldBeWarning();
        }

        [TestCase("develop")]
        [TestCase("nightly")]
        public void should_return_error_when_branch_is_v1(string branch)
        {
            GivenValidBranch(branch);

            Subject.Check().ShouldBeError();
        }

        [TestCase("aphrodite")]
        [TestCase("Aphrodite")]
        public void should_return_no_warning_when_branch_valid(string branch)
        {
            GivenValidBranch(branch);

            Subject.Check().ShouldBeOk();
        }
    }
}
