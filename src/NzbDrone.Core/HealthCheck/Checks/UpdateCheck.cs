using System;
using System.IO;
using Microsoft.Extensions.Options;
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
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileOptions;
        private readonly IOsInfo _osInfo;

        public UpdateCheck(IDiskProvider diskProvider,
                           IAppFolderInfo appFolderInfo,
                           ICheckUpdateService checkUpdateService,
                           IOptionsMonitor<ConfigFileOptions> configFileOptions,
                           IOsInfo osInfo,
                           ILocalizationService localizationService)
            : base(localizationService)
        {
            _diskProvider = diskProvider;
            _appFolderInfo = appFolderInfo;
            _checkUpdateService = checkUpdateService;
            _configFileOptions = configFileOptions;
            _osInfo = osInfo;
        }

        public override HealthCheck Check()
        {
            var startupFolder = _appFolderInfo.StartUpFolder;
            var uiFolder = Path.Combine(startupFolder, "UI");

            if ((OsInfo.IsWindows || _configFileOptions.CurrentValue.UpdateAutomatically) &&
                _configFileOptions.CurrentValue.UpdateMechanism == UpdateMechanism.BuiltIn &&
                !_osInfo.IsDocker)
            {
                if (OsInfo.IsOsx && startupFolder.GetAncestorFolders().Contains("AppTranslocation"))
                {
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Error,
                        string.Format(_localizationService.GetLocalizedString("UpdateCheckStartupTranslocationMessage"), startupFolder),
                        "#cannot-install-update-because-startup-folder-is-in-an-app-translocation-folder.");
                }

                if (!_diskProvider.FolderWritable(startupFolder))
                {
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Error,
                        string.Format(_localizationService.GetLocalizedString("UpdateCheckStartupNotWritableMessage"), startupFolder, Environment.UserName),
                        "#cannot-install-update-because-startup-folder-is-not-writable-by-the-user");
                }

                if (!_diskProvider.FolderWritable(uiFolder))
                {
                    return new HealthCheck(GetType(),
                        HealthCheckResult.Error,
                        string.Format(_localizationService.GetLocalizedString("UpdateCheckUINotWritableMessage"), uiFolder, Environment.UserName),
                        "#cannot-install-update-because-ui-folder-is-not-writable-by-the-user");
                }
            }

            if (BuildInfo.BuildDateTime < DateTime.UtcNow.AddDays(-14) && _checkUpdateService.AvailableUpdate() != null)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, _localizationService.GetLocalizedString("UpdateAvailable"), "#new-update-is-available");
            }

            return new HealthCheck(GetType());
        }
    }
}
