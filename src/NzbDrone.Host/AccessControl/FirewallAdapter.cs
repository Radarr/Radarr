using System;
using System.Linq;
using Microsoft.Extensions.Options;
using NetFwTypeLib;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace Radarr.Host.AccessControl
{
    public interface IFirewallAdapter
    {
        void MakeAccessible();
    }

    public class FirewallAdapter : IFirewallAdapter
    {
        private const NET_FW_PROFILE_TYPE_ FIREWALL_PROFILE = NET_FW_PROFILE_TYPE_.NET_FW_PROFILE_STANDARD;

        private readonly IOptionsMonitor<ConfigFileOptions> _configFileProvider;
        private readonly Logger _logger;

        public FirewallAdapter(IOptionsMonitor<ConfigFileOptions> configFileProvider, Logger logger)
        {
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public void MakeAccessible()
        {
            if (IsFirewallEnabled())
            {
                if (!IsNzbDronePortOpen(_configFileProvider.CurrentValue.Port))
                {
                    _logger.Debug("Opening Port for Radarr: {0}", _configFileProvider.CurrentValue.Port);
                    OpenFirewallPort(_configFileProvider.CurrentValue.Port);
                }

                if (_configFileProvider.CurrentValue.EnableSsl && !IsNzbDronePortOpen(_configFileProvider.CurrentValue.SslPort))
                {
                    _logger.Debug("Opening SSL Port for Radarr: {0}", _configFileProvider.CurrentValue.SslPort);
                    OpenFirewallPort(_configFileProvider.CurrentValue.SslPort);
                }
            }
        }

        private bool IsNzbDronePortOpen(int port)
        {
            try
            {
                var netFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);

                var mgr = (INetFwMgr)Activator.CreateInstance(netFwMgrType);
                var ports = mgr.LocalPolicy.GetProfileByType(FIREWALL_PROFILE).GloballyOpenPorts;

                return ports.Cast<INetFwOpenPort>().Any(p => p.Port == port);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to check for open port in firewall");
            }

            return false;
        }

        private void OpenFirewallPort(int portNumber)
        {
            try
            {
                var type = Type.GetTypeFromProgID("HNetCfg.FWOpenPort", false);
                var port = (INetFwOpenPort)Activator.CreateInstance(type);

                port.Port = portNumber;
                port.Name = "NzbDrone";
                port.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
                port.Enabled = true;

                var netFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
                var mgr = (INetFwMgr)Activator.CreateInstance(netFwMgrType);

                //Open the port for the standard profile, should help when the user has multiple network adapters
                mgr.LocalPolicy.GetProfileByType(FIREWALL_PROFILE).GloballyOpenPorts.Add(port);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to open port in firewall for Radarr " + portNumber);
            }
        }

        private bool IsFirewallEnabled()
        {
            if (OsInfo.IsNotWindows)
            {
                return false;
            }

            try
            {
                var netFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
                var mgr = (INetFwMgr)Activator.CreateInstance(netFwMgrType);
                return mgr.LocalPolicy.GetProfileByType(FIREWALL_PROFILE).FirewallEnabled;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to check if the firewall is enabled");
                return false;
            }
        }
    }
}
