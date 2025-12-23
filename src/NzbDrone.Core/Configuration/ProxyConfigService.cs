using NzbDrone.Common.Http.Proxy;

namespace NzbDrone.Core.Configuration
{
    public class ProxyConfigService : IProxyConfigService
    {
        private readonly IConfigService _configService;

        public ProxyConfigService(IConfigService configService)
        {
            _configService = configService;
        }

        public bool ProxyEnabled => _configService.ProxyEnabled;

        public ProxyType ProxyType => _configService.ProxyType;

        public string ProxyHostname => _configService.ProxyHostname;

        public int ProxyPort => _configService.ProxyPort;

        public string ProxyUsername => _configService.ProxyUsername;

        public string ProxyPassword => _configService.ProxyPassword;

        public string ProxyBypassFilter => _configService.ProxyBypassFilter;

        public bool ProxyBypassLocalAddresses => _configService.ProxyBypassLocalAddresses;
    }
}
