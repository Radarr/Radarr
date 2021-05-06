using System;
using System.IO;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ConfigFileSavedEvent))]
    public class UpdateCheck : HealthCheckBase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly ICheckUpdateService _checkUpdateService;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IOsInfo _osInfo;

        public UpdateCheck(IDiskProvider diskProvider,
                           IAppFolderInfo appFolderInfo,
                           ICheckUpdateService checkUpdateService,
                           IConfigFileProvider configFileProvider,
                           IOsInfo osInfo,
                           ILocalizationService localizationService)
            : base(localizationService)
        {
            _diskProvider = diskProvider;
            _appFolderInfo = appFolderInfo;
            _checkUpdateService = checkUpdateService;
            _configFileProvider = configFileProvider;
            _osInfo = osInfo;
        }

        public override HealthCheck Check()
        {
            var startupFolder = _appFolderInfo.StartUpFolder;
            var uiFolder = Path.Combine(startupFolder, "UI");

            if ((OsInfo.IsWindows || _configFileProvider.UpdateAutomatically) &&
                _configFileProvider.UpdateMechanism == UpdateMechanism.BuiltIn &&
                !_osInfo.IsDocker)
            {
                if (OsInfo.IsOsx && startupFolder.GetAncestorFolders().Contains("AppTranslocation"))
                {
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Error,
                        string.Format(_localizationService.GetLocalizedString("UpdateCheckStartupTranslocationMessage"), startupFolder),
                        "#cannot_install_update_because_startup_folder_is_in_an_app_translocation_folder.");
                }

                if (!_diskProvider.FolderWritable(startupFolder))
                {
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Error,
                        string.Format(_localizationService.GetLocalizedString("UpdateCheckStartupNotWritableMessage"), startupFolder, Environment.UserName),
                        "#cannot_install_update_because_startup_folder_is_not_writable_by_the_user");
                }

                if (!_diskProvider.FolderWritable(uiFolder))
                {
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Error,
                        string.Format(_localizationService.GetLocalizedString("UpdateCheckUINotWritableMessage"), uiFolder, Environment.UserName),
                        "#cannot_install_update_because_ui_folder_is_not_writable_by_the_user");
                }
            }

            if (BuildInfo.BuildDateTime < DateTime.UtcNow.AddDays(-14) && _checkUpdateService.AvailableUpdate() != null)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, _localizationService.GetLocalizedString("UpdateAvailable"), "#new_update_is_available");
            }

            return new HealthCheck(GetType());
        }
    }
}
