using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Host;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class AllowedHostsMiddlewareFixture
    {
        private Mock<IConfigFileProvider> _configFileProvider;
        private bool _nextCalled;

        [SetUp]
        public void Setup()
        {
            _configFileProvider = new Mock<IConfigFileProvider>();
            _nextCalled = false;
        }

        private Task<int> Invoke(string allowedHosts, string hostHeader)
        {
            _configFileProvider.SetupGet(c => c.AllowedHosts).Returns(allowedHosts);

            return InvokeHost(hostHeader);
        }

        private Task<int> InvokeHost(string hostHeader)
        {
            var context = new DefaultHttpContext();

            if (hostHeader is not null)
            {
                context.Request.Headers["Host"] = hostHeader;
            }

            var middleware = new AllowedHostsMiddleware(
                _ =>
                {
                    _nextCalled = true;

                    return Task.CompletedTask;
                },
                _configFileProvider.Object);

            return middleware.InvokeAsync(context).ContinueWith(t => context.Response.StatusCode);
        }

        private void ShouldBeAllowed(string allowedHosts, string hostHeader)
        {
            Invoke(allowedHosts, hostHeader).GetAwaiter().GetResult().Should().NotBe(StatusCodes.Status400BadRequest);
            _nextCalled.Should().BeTrue();
        }

        private void ShouldBeRejected(string allowedHosts, string hostHeader)
        {
            Invoke(allowedHosts, hostHeader).GetAwaiter().GetResult().Should().Be(StatusCodes.Status400BadRequest);
            _nextCalled.Should().BeFalse();
        }

        [TestCase(null)]
        [TestCase("")]
        public void should_allow_request_without_host_header(string hostHeader)
        {
            ShouldBeAllowed("sonarr.local, 192.168.0.0/16", hostHeader);
        }

        [TestCase("sonarr.local", "other.local")]
        [TestCase("sonarr.local", "192.168.1.5")]
        [TestCase("0.0.0.0", "8.8.8.8")]
        [TestCase("", "anything.local")]
        public void should_leave_filtering_to_built_in_host_filtering_when_no_subnets_are_configured(string allowedHosts, string hostHeader)
        {
            ShouldBeAllowed(allowedHosts, hostHeader);
        }

        [TestCase("192.168.0.0/16", "192.168.0.0")]
        [TestCase("192.168.0.0/16", "192.168.1.5")]
        [TestCase("192.168.0.0/16", "192.168.255.255:7878")]
        [TestCase("10.0.0.0/8,172.16.0.0/12,192.168.0.0/16", "10.20.30.40")]
        [TestCase("10.0.0.0/8,172.16.0.0/12,192.168.0.0/16", "172.16.0.1:7878")]
        [TestCase("10.0.0.0/8,172.16.0.0/12,192.168.0.0/16", "192.168.5.5")]
        [TestCase("sonarr.local, 192.168.0.0/16", "192.168.1.5")]
        [TestCase("sonarr.local, 192.168.0.0/16", "sonarr.local:7878")]
        [TestCase("2001:db8::/32", "[2001:db8::1]")]
        [TestCase("2001:db8::/32", "[2001:db8::1]:7878")]
        public void should_allow_hosts_within_configured_subnets(string allowedHosts, string hostHeader)
        {
            ShouldBeAllowed(allowedHosts, hostHeader);
        }

        [TestCase("sonarr.local, 192.168.0.0/16", "SONARR.LOCAL")]
        [TestCase("*.example.com, 192.168.0.0/16", "foo.example.com")]
        [TestCase("sonarr.local, 192.168.0.0/16", "sonarr.local:7878")]
        public void should_allow_matching_hosts_case_insensitively_and_with_wildcards(string allowedHosts, string hostHeader)
        {
            ShouldBeAllowed(allowedHosts, hostHeader);
        }

        [TestCase("sonarr.local, 192.168.0.0/16", "OTHER.LOCAL")]
        [TestCase("*.example.com, 192.168.0.0/16", "evilexample.com")]
        [TestCase("2001:db8::/32", "8.8.8.8")]
        [TestCase("10.0.0.0/8", "[2001:db8::1]")]
        [TestCase("192.168.0.0/16", "192.169.0.0")]
        [TestCase("192.168.0.0/16", "8.8.8.8")]
        [TestCase("192.168.0.0/16", "192.168.0.0/16")]
        [TestCase("10.0.0.0/8,172.16.0.0/12,192.168.0.0/16", "100.64.0.1")]
        [TestCase("sonarr.local, 192.168.0.0/16", "other.local")]
        [TestCase("2001:db8::/32", "[2001:db9::1]")]
        [TestCase("2001:db8::/32", "sonarr.local")]
        public void should_reject_hosts_outside_configured_subnets(string allowedHosts, string hostHeader)
        {
            ShouldBeRejected(allowedHosts, hostHeader);
        }

        [Test]
        public void should_allow_any_host_when_subnets_are_configured_with_wildcard()
        {
            ShouldBeAllowed("*, 192.168.0.0/16", "anything.local");
        }

        [Test]
        public void should_re_read_allowed_hosts_on_every_request()
        {
            _configFileProvider.SetupSequence(c => c.AllowedHosts)
                .Returns("192.168.0.0/16")
                .Returns("other.local");

            InvokeHost("other.local").GetAwaiter().GetResult().Should().Be(StatusCodes.Status400BadRequest);
            _nextCalled.Should().BeFalse();

            InvokeHost("other.local").GetAwaiter().GetResult().Should().NotBe(StatusCodes.Status400BadRequest);
            _nextCalled.Should().BeTrue();
        }

        [TestCase("localhost")]
        [TestCase("127.0.0.1")]
        [TestCase("[::1]")]
        [TestCase("localhost:7878")]
        public void should_always_allow_loopback_hosts(string hostHeader)
        {
            ShouldBeAllowed("192.168.0.0/16", hostHeader);
        }

        [Test]
        public void should_always_allow_the_server_host_name()
        {
            ShouldBeAllowed("192.168.0.0/16", Environment.MachineName);
        }

        [TestCase("0.0.0.0, 192.168.0.0/16", "8.8.8.8")]
        [TestCase("[::], 192.168.0.0/16", "sonarr.local")]
        public void should_not_treat_bind_addresses_as_wildcards(string allowedHosts, string hostHeader)
        {
            ShouldBeRejected(allowedHosts, hostHeader);
        }
    }
}
