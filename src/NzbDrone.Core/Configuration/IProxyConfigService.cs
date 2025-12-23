using NzbDrone.Common.Http.Proxy;

namespace NzbDrone.Core.Configuration
{
    public interface IProxyConfigService
    {
        bool ProxyEnabled { get; }
        ProxyType ProxyType { get; }
        string ProxyHostname { get; }
        int ProxyPort { get; }
        string ProxyUsername { get; }
        string ProxyPassword { get; }
        string ProxyBypassFilter { get; }
        bool ProxyBypassLocalAddresses { get; }
    }
}
