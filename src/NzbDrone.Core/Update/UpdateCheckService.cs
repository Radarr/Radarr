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
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileOptions;

        public CheckUpdateService(IUpdatePackageProvider updatePackageProvider,
                                  IOptionsMonitor<ConfigFileOptions> configFileOptions)
        {
            _updatePackageProvider = updatePackageProvider;
            _configFileOptions = configFileOptions;
        }

        public UpdatePackage AvailableUpdate()
        {
            return _updatePackageProvider.GetLatestUpdate(_configFileOptions.CurrentValue.Branch, BuildInfo.Version);
        }
    }
}
