using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck
{
    [TestFixture]
    public class HealthCheckFixture : CoreTest
    {
        private const string WikiRoot = "https://wiki.servarr.com/";

        [TestCase("I blew up because of some weird user mistake", null, WikiRoot + "radarr/system#i-blew-up-because-of-some-weird-user-mistake")]
        [TestCase("I blew up because of some weird user mistake", "#my-health-check", WikiRoot + "radarr/system#my-health-check")]
        [TestCase("I blew up because of some weird user mistake", "custom-page#my-health-check", WikiRoot + "radarr/custom-page#my-health-check")]
        public void should_format_wiki_url(string message, string wikiFragment, string expectedUrl)
        {
            var subject = new NzbDrone.Core.HealthCheck.HealthCheck(typeof(HealthCheckBase), HealthCheckResult.Warning, HealthCheckReason.ServerNotification, message, wikiFragment);

            subject.WikiUrl.Should().Be(expectedUrl);
        }

        [Test]
        public void should_format_wiki_url_with_turkish_culture()
        {
            var currentCulture = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

                var subject = new NzbDrone.Core.HealthCheck.HealthCheck(typeof(HealthCheckBase), HealthCheckResult.Warning, HealthCheckReason.ServerNotification, "Indexers unavailable due to failures", null);

                subject.WikiUrl.Should().Be(WikiRoot + "radarr/system#indexers-unavailable-due-to-failures");
            }
            finally
            {
                CultureInfo.CurrentCulture = currentCulture;
            }
        }
    }
}
