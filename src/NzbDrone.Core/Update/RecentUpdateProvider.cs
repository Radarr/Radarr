using System.Collections.Generic;
using Microsoft.Extensions.Options;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Update.History;

namespace NzbDrone.Core.Update
{
    public interface IRecentUpdateProvider
    {
        List<UpdatePackage> GetRecentUpdatePackages();
    }

    public class RecentUpdateProvider : IRecentUpdateProvider
    {
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileOptions;
        private readonly IUpdatePackageProvider _updatePackageProvider;
        private readonly IUpdateHistoryService _updateHistoryService;

        public RecentUpdateProvider(IOptionsMonitor<ConfigFileOptions> configFileOptions,
                                    IUpdatePackageProvider updatePackageProvider,
                                    IUpdateHistoryService updateHistoryService)
        {
            _configFileOptions = configFileOptions;
            _updatePackageProvider = updatePackageProvider;
            _updateHistoryService = updateHistoryService;
        }

        public List<UpdatePackage> GetRecentUpdatePackages()
        {
            var branch = _configFileOptions.CurrentValue.Branch;
            var version = BuildInfo.Version;
            var prevVersion = _updateHistoryService.PreviouslyInstalled();
            return _updatePackageProvider.GetRecentUpdates(branch, version, prevVersion);
        }
    }
}
