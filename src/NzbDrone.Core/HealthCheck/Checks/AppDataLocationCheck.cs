using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class AppDataLocationCheck : HealthCheckBase
    {
        private readonly IAppFolderInfo _appFolderInfo;

        public AppDataLocationCheck(IAppFolderInfo appFolderInfo, ILocalizationService localizationService)
            : base(localizationService)
        {
            _appFolderInfo = appFolderInfo;
        }

        public override HealthCheck Check()
        {
            if (_appFolderInfo.StartUpFolder.IsParentPath(_appFolderInfo.AppDataFolder) ||
                _appFolderInfo.StartUpFolder.PathEquals(_appFolderInfo.AppDataFolder))
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, _localizationService.GetLocalizedString("AppDataLocationHealthCheckMessage"), "#updating_will_not_be_possible_to_prevent_deleting_appdata_on_update");
            }

            return new HealthCheck(GetType());
        }
    }
}
