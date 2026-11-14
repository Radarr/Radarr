using FluentAssertions;
using Microsoft.AspNetCore.HostFiltering;
using Moq;
using NLog;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Host;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class ConfigureHostFilteringOptionsFixture
    {
        private static HostFilteringOptions GetOptions(string allowedHosts)
        {
            var configFileProvider = new Mock<IConfigFileProvider>();
            configFileProvider.SetupGet(c => c.AllowedHosts).Returns(allowedHosts);

            var options = new HostFilteringOptions();

            new ConfigureHostFilteringOptions(configFileProvider.Object, LogManager.CreateNullLogger()).Configure(options);

            return options;
        }

        [TestCase("")]
        [TestCase("0.0.0.0")]
        [TestCase("::")]
        [TestCase("0.0.0.0, [::]")]
        public void should_allow_any_host_when_only_bind_addresses_are_configured(string allowedHosts)
        {
            GetOptions(allowedHosts).AllowedHosts.Should().BeEquivalentTo("*");
        }

        [Test]
        public void should_skip_bind_addresses_when_host_names_are_configured()
        {
            var hosts = GetOptions("sonarr.local, 0.0.0.0, [::]").AllowedHosts;

            hosts.Should().Contain("sonarr.local");
            hosts.Should().Contain("localhost");
            hosts.Should().NotContain("0.0.0.0");
            hosts.Should().NotContain("[::]");
            hosts.Should().NotContain("*");
        }

        [TestCase("192.168.0.0/16")]
        [TestCase("10.0.0.0/8,172.16.0.0/12,192.168.0.0/16")]
        [TestCase("sonarr.local, 192.168.0.0/16")]
        public void should_hand_filtering_over_to_allowed_hosts_middleware_when_subnets_are_configured(string allowedHosts)
        {
            GetOptions(allowedHosts).AllowedHosts.Should().BeEquivalentTo("*");
        }
    }
}
