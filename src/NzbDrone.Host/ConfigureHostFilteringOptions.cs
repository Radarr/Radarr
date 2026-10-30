using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.Extensions.Options;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Host
{
    public class ConfigureHostFilteringOptions : IConfigureOptions<HostFilteringOptions>
    {
        // Always allow localhost, loopback addresses and the hostname of the server, even if
        // they aren't configured in the allowed hosts list so users can access the UI using them.
        internal static readonly string[] LoopbackHosts = { "localhost", "127.0.0.1", "[::1]" };

        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        public ConfigureHostFilteringOptions(IConfigFileProvider configFileProvider, Logger logger)
        {
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public void Configure(HostFilteringOptions options)
        {
            var allowedHosts = AllowedHostsParser.Parse(_configFileProvider.AllowedHosts);
            var bindAddresses = allowedHosts.Where(AllowedHostsParser.IsBindAddress).ToList();

            if (bindAddresses.Any())
            {
                _logger.Warn("Ignoring {0} in Allowed Hosts, bind addresses are not host names", string.Join(", ", bindAddresses));

                allowedHosts = allowedHosts.Except(bindAddresses).ToList();
            }

            options.AllowEmptyHosts = true;

            if (allowedHosts.Empty())
            {
                options.AllowedHosts = new List<string> { "*" };

                _logger.Info("Allowed Hosts is not configured, accepting requests for any host");

                return;
            }

            // Built-in host filtering matches hosts as strings only, so it can't handle subnet
            // entries. If any are configured, let the built-in filtering accept all hosts and
            // enforce the full list, subnets included, in AllowedHostsMiddleware instead.
            if (allowedHosts.Any(AllowedHostsParser.IsSubnet))
            {
                options.AllowedHosts = new List<string> { "*" };

                _logger.Info("Allowed Hosts includes subnets, accepting requests for hosts: {0} and any IP address within those subnets", string.Join(", ", allowedHosts));

                return;
            }

            options.AllowedHosts = allowedHosts.Concat(LoopbackHosts).Concat(GetServerHostNames(_logger)).Distinct().ToList();

            _logger.Info("Accepting requests for hosts: {0}", string.Join(", ", options.AllowedHosts));
        }

        internal static List<string> GetServerHostNames(Logger logger)
        {
            var hostNames = new List<string>();

            if (Environment.MachineName.IsNotNullOrWhiteSpace())
            {
                hostNames.Add(Environment.MachineName);
            }

            try
            {
                var hostName = Dns.GetHostName();

                if (hostName.IsNotNullOrWhiteSpace())
                {
                    hostNames.Add(hostName);
                }
            }
            catch (Exception ex)
            {
                logger.Debug(ex, "Unable to get the hostname of the server");
            }

            return hostNames;
        }
    }
}
