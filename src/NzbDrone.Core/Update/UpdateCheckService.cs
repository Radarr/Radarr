using Microsoft.Extensions.Options;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Update
{
    public interface ICheckUpdateService
    {
        UpdatePackage AvailableUpdate();
    }

    public class CheckUpdateService : ICheckUpdateService
    {
        private readonly IUpdatePackageProvider _updatePackageProvider;
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileProvider;

        public CheckUpdateService(IUpdatePackageProvider updatePackageProvider,
                                  IOptionsMonitor<ConfigFileOptions> configFileProvider)
        {
            _updatePackageProvider = updatePackageProvider;
            _configFileProvider = configFileProvider;
        }

        public UpdatePackage AvailableUpdate()
        {
            return _updatePackageProvider.GetLatestUpdate(_configFileProvider.CurrentValue.Branch, BuildInfo.Version);
        }
    }
}
