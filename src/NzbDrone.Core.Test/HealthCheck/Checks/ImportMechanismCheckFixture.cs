using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class ImportMechanismCheckFixture : CoreTest<ImportMechanismCheck>
    {

        private void GivenCompletedDownloadHandling(bool? enabled = null)
        {
            if (enabled.HasValue)
            {
                Mocker.GetMock<IConfigService>()
                      .SetupGet(s => s.EnableCompletedDownloadHandling)
                      .Returns(enabled.Value);
            }
        }

        [Test]
        public void should_return_warning_when_completeddownloadhandling_false()
        {
            GivenCompletedDownloadHandling(false);
            
            Subject.Check().ShouldBeWarning();
        }
        
        [Test]
        public void should_return_ok_when_no_issues_found()
        {
            GivenCompletedDownloadHandling(true);

            Subject.Check().ShouldBeOk();
        }
    }
}
