using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Exceptions;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Common.EnvironmentInfo
{
    public interface IAppFolderFactory
    {
        void Register();
    }

    public class AppFolderFactory : IAppFolderFactory
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public AppFolderFactory(IAppFolderInfo appFolderInfo,
                                IStartupContext startupContext,
                                IDiskProvider diskProvider,
                                IDiskTransferService diskTransferService)
        {
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
            _logger = NzbDroneLogger.GetLogger(this);
        }

        public void Register()
        {
            try
            {
                _diskProvider.EnsureFolder(_appFolderInfo.AppDataFolder);
            }
            catch (UnauthorizedAccessException)
            {
                throw new ReadarrStartupException("Cannot create AppFolder, Access to the path {0} is denied", _appFolderInfo.AppDataFolder);
            }

            if (OsInfo.IsWindows)
            {
                SetPermissions();
            }

            if (!_diskProvider.FolderWritable(_appFolderInfo.AppDataFolder))
            {
                throw new ReadarrStartupException("AppFolder {0} is not writable", _appFolderInfo.AppDataFolder);
            }

            InitializeMonoApplicationData();
        }

        private void SetPermissions()
        {
            try
            {
                _diskProvider.SetEveryonePermissions(_appFolderInfo.AppDataFolder);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Coudn't set app folder permission");
            }
        }

        private void InitializeMonoApplicationData()
        {
            if (OsInfo.IsWindows)
            {
                return;
            }

            try
            {
                // It seems that DoNotVerify is the mono behaviour even though .net docs specify a blank string
                // should be returned if the data doesn't exist.  For compatibility with .net core, explicitly
                // set DoNotVerify (which makes sense given we're explicitly checking that the folder exists)
                var configHome = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify);
                if (configHome.IsNullOrWhiteSpace() ||
                    configHome == "/.config" ||
                    (configHome.EndsWith("/.config") && !_diskProvider.FolderExists(configHome.GetParentPath())) ||
                    !_diskProvider.FolderExists(configHome))
                {
                    // Tell mono/netcore to use appData/.config as ApplicationData folder.
                    Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", Path.Combine(_appFolderInfo.AppDataFolder, ".config"));
                }

                var dataHome = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
                if (dataHome.IsNullOrWhiteSpace() ||
                    dataHome == "/.local/share" ||
                    (dataHome.EndsWith("/.local/share") && !_diskProvider.FolderExists(dataHome.GetParentPath().GetParentPath())) ||
                    !_diskProvider.FolderExists(dataHome))
                {
                    // Tell mono/netcore to use appData/.config/share as LocalApplicationData folder.
                    Environment.SetEnvironmentVariable("XDG_DATA_HOME", Path.Combine(_appFolderInfo.AppDataFolder, ".config/share"));
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to initialize the mono config directory.");
            }
        }
    }
}
