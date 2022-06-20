using System;
using Microsoft.Extensions.Options;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Configuration;

namespace Radarr.Host
{
    public interface IBrowserService
    {
        void LaunchWebUI();
    }

    public class BrowserService : IBrowserService
    {
        private readonly IProcessProvider _processProvider;
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileOptions;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly Logger _logger;

        public BrowserService(IProcessProvider processProvider, IOptionsMonitor<ConfigFileOptions> configFileOptions, IRuntimeInfo runtimeInfo, Logger logger)
        {
            _processProvider = processProvider;
            _configFileOptions = configFileOptions;
            _runtimeInfo = runtimeInfo;
            _logger = logger;
        }

        public void LaunchWebUI()
        {
            var url = string.Format("http://localhost:{0}", _configFileOptions.CurrentValue.Port);
            try
            {
                if (_runtimeInfo.IsUserInteractive)
                {
                    _logger.Info("Starting default browser. {0}", url);
                    _processProvider.OpenDefaultBrowser(url);
                }
                else
                {
                    _logger.Debug("non-interactive runtime. Won't attempt to open browser.");
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't open default browser to " + url);
            }
        }
    }
}
