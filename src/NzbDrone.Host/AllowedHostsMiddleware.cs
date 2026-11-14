using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Network;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Host
{
    public class AllowedHostsMiddleware
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(AllowedHostsMiddleware));

        private readonly RequestDelegate _next;
        private readonly IConfigFileProvider _configFileProvider;
        private string _lastAllowedHosts;

        public AllowedHostsMiddleware(RequestDelegate next, IConfigFileProvider configFileProvider)
        {
            _next = next;
            _configFileProvider = configFileProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var host = context.Request.Headers.Host.ToString();

            if (host.IsNullOrWhiteSpace() || IsAllowedHost(host))
            {
                await _next(context);

                return;
            }

            Logger.Info("Host {0} not in Allowed Hosts, rejecting request", host);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        private bool IsAllowedHost(string host)
        {
            var allowedHosts = AllowedHostsParser.Parse(_configFileProvider.AllowedHosts);

            if (allowedHosts.All(h => !AllowedHostsParser.IsSubnet(h)))
            {
                // No subnets configured, the built-in host filtering already accepted the request.
                return true;
            }

            if (allowedHosts.Contains("*"))
            {
                return true;
            }

            // Bind addresses are not host names, ConfigureHostFilteringOptions already warned about them.
            var hostNames = allowedHosts.Where(h => !AllowedHostsParser.IsSubnet(h) && !AllowedHostsParser.IsBindAddress(h))
                                        .Concat(ConfigureHostFilteringOptions.LoopbackHosts)
                                        .Concat(ConfigureHostFilteringOptions.GetServerHostNames(Logger))
                                        .ToList();

            LogAllowedHostsIfChanged(allowedHosts.Where(AllowedHostsParser.IsSubnet).Concat(hostNames).ToList());

            if (HostString.MatchesAny(new StringSegment(host), hostNames.Select(h => new StringSegment(new HostString(h).ToUriComponent())).ToList()))
            {
                return true;
            }

            return TryGetHostIpAddress(host, out var address) &&
                   allowedHosts.Where(AllowedHostsParser.IsSubnet)
                               .Any(h => IPNetworkParser.TryParse(h, out var network, out var prefixLength) &&
                                         new IPNetwork(network, prefixLength).Contains(address));
        }

        private void LogAllowedHostsIfChanged(List<string> hostNames)
        {
            var value = string.Join(", ", hostNames);

            if (value == _lastAllowedHosts)
            {
                return;
            }

            _lastAllowedHosts = value;

            Logger.Info("Accepting requests for hosts: {0} and any IP address within those subnets", value);
        }

        private static bool TryGetHostIpAddress(string host, out IPAddress address)
        {
            address = null;

            // HostString strips the port and keeps brackets around IPv6 addresses.
            var hostPart = new HostString(host).Host;

            if (hostPart.StartsWith('[') && hostPart.EndsWith(']'))
            {
                hostPart = hostPart.Substring(1, hostPart.Length - 2);
            }

            return hostPart.IsNotNullOrWhiteSpace() && IPAddress.TryParse(hostPart, out address);
        }
    }
}
