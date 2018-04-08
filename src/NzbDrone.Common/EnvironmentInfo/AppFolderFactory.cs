using System;
using System.Security.AccessControl;
using System.Security.Principal;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Exceptions;
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
                throw new LidarrStartupException("Cannot create AppFolder, Access to the path {0} is denied", _appFolderInfo.AppDataFolder);
            }
            

            if (OsInfo.IsWindows)
            {
                SetPermissions();
            }

            if (!_diskProvider.FolderWritable(_appFolderInfo.AppDataFolder))
            {
                throw new LidarrStartupException("AppFolder {0} is not writable", _appFolderInfo.AppDataFolder);
            }
        }

        private void SetPermissions()
        {
            try
            {
                _diskProvider.SetPermissions(_appFolderInfo.AppDataFolder, WellKnownSidType.WorldSid, FileSystemRights.Modify, AccessControlType.Allow);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Coudn't set app folder permission");
            }
        }
    }
}
