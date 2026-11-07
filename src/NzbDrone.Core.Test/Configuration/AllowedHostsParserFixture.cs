using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Configuration
{
    [TestFixture]
    public class AllowedHostsParserFixture : TestBase
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void should_return_empty_list_for_null_or_empty(string value)
        {
            AllowedHostsParser.Parse(value).Should().BeEmpty();
        }

        [Test]
        public void should_split_on_comma_and_semicolon_and_trim_whitespace()
        {
            var result = AllowedHostsParser.Parse("radarr.local, 192.168.1.5; myhost");

            result.Should().BeEquivalentTo("radarr.local", "192.168.1.5", "myhost");
        }

        [Test]
        public void should_wrap_bare_ipv6_addresses_in_brackets()
        {
            var result = AllowedHostsParser.Parse("::1, 2001:db8::1, [fe80::1]");

            result.Should().BeEquivalentTo("[::1]", "[2001:db8::1]", "[fe80::1]");
        }

        [Test]
        public void should_not_duplicate_bare_and_bracketed_ipv6_addresses()
        {
            var result = AllowedHostsParser.Parse("::1,[::1]");

            result.Should().BeEquivalentTo("[::1]");
        }

        [Test]
        public void should_remove_empty_entries_and_duplicates()
        {
            var result = AllowedHostsParser.Parse("a,,b, a ");

            result.Should().BeEquivalentTo("a", "b");
        }

        [TestCase("radarr.local")]
        [TestCase("radarr.example.co.uk")]
        [TestCase("my-host")]
        [TestCase("192.168.1.5")]
        [TestCase("[::1]")]
        [TestCase("[2001:db8::1]")]
        [TestCase("2001:db8::1")]
        [TestCase("*.example.com")]
        [TestCase("*.example.co.uk")]
        [TestCase("*.subdomain.example.com")]
        [TestCase("*.subdomain.example.co.uk")]
        [TestCase("192.168.0.0/16")]
        [TestCase("10.0.0.0/8")]
        [TestCase("172.16.0.0/12")]
        [TestCase("fd00::/8")]
        [TestCase("2001:db8::/32")]
        [TestCase("[::1]/128")]
        public void should_be_valid_host(string host)
        {
            AllowedHostsParser.IsValidHost(host).Should().BeTrue();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        [TestCase("http://radarr.local")]
        [TestCase("radarr.local:8989")]
        [TestCase("radarr local")]
        [TestCase("radarr.*")]
        [TestCase("*")]
        [TestCase("*.")]
        [TestCase("*.192.168.1.5")]
        [TestCase("*.[::1]")]
        [TestCase("*.*.example.com")]
        [TestCase("[not-an-address]")]
        [TestCase(".example.com")]
        [TestCase(".example.co.uk")]
        [TestCase("192.168.1.5/16")]
        [TestCase("10.0.0.0/33")]
        [TestCase("10.0.0.0/")]
        [TestCase("10.0.0.0/abc")]
        [TestCase("192.168.0.0/16/8")]
        public void should_not_be_valid_host(string host)
        {
            AllowedHostsParser.IsValidHost(host).Should().BeFalse();
        }

        [TestCase("0.0.0.0")]
        [TestCase("::")]
        [TestCase("[::]")]
        public void should_be_bind_address(string host)
        {
            AllowedHostsParser.Parse(host).Should().ContainSingle().Which.Should().Match(h => AllowedHostsParser.IsBindAddress(h));
        }

        [TestCase("*")]
        [TestCase("sonarr.local")]
        [TestCase("*.example.com")]
        [TestCase("192.168.1.5")]
        [TestCase("[::1]")]
        public void should_not_be_bind_address(string host)
        {
            AllowedHostsParser.IsBindAddress(host).Should().BeFalse();
        }

        [TestCase("192.168.0.0/16", true)]
        [TestCase("10.0.0.0/8", true)]
        [TestCase("172.16.0.0/12", true)]
        [TestCase("fd00::/8", true)]
        [TestCase("192.168.1.5", false)]
        [TestCase("192.168.1.5/16", false)]
        [TestCase("sonarr.local", false)]
        [TestCase(null, false)]
        public void should_detect_subnet_entries(string host, bool expected)
        {
            AllowedHostsParser.IsSubnet(host).Should().Be(expected);
        }
    }
}
