using System;
using System.IO;
using System.Linq;
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
        private readonly IStartupContext _startupContext;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _diskTransferService;
        private readonly Logger _logger;

        public AppFolderFactory(IAppFolderInfo appFolderInfo,
                                        IStartupContext startupContext,
                                        IDiskProvider diskProvider,
                                        IDiskTransferService diskTransferService)
        {
            _appFolderInfo = appFolderInfo;
            _startupContext = startupContext;
            _diskProvider = diskProvider;
            _diskTransferService = diskTransferService;
            _logger = NzbDroneLogger.GetLogger(this);
        }

        public void Register()
        {
            try
            {
                MigrateAppDataFolder();
                _diskProvider.EnsureFolder(_appFolderInfo.AppDataFolder);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new RadarrStartupException(ex, "Cannot create AppFolder, Access to the path {0} is denied", _appFolderInfo.AppDataFolder);
            }
            

            if (OsInfo.IsWindows)
            {
                SetPermissions();
            }

            if (!_diskProvider.FolderWritable(_appFolderInfo.AppDataFolder))
            {
                throw new RadarrStartupException("AppFolder {0} is not writable", _appFolderInfo.AppDataFolder);
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

        private void MigrateAppDataFolder()
        {
            try
            {
                var oldDbFile = Path.Combine(_appFolderInfo.AppDataFolder, "nzbdrone.db");

                if (_startupContext.Args.ContainsKey(StartupContext.APPDATA))
                {
                    if (_diskProvider.FileExists(_appFolderInfo.GetDatabase())) return;
                    if (!_diskProvider.FileExists(oldDbFile)) return;

                    _diskProvider.MoveFile(oldDbFile, _appFolderInfo.GetDatabase());
                    CleanupSqLiteRollbackFiles();
                    RemovePidFile();
                }

                // Exit if a radarr.db already exists
                if (_diskProvider.FileExists(_appFolderInfo.GetDatabase())) return;

                // Rename the DB file
                if (_diskProvider.FileExists(oldDbFile))
                {
                    _diskProvider.MoveFile(oldDbFile, _appFolderInfo.GetDatabase());
                }

                // Remove SQLite rollback files
                CleanupSqLiteRollbackFiles();

                // Remove Old PID file
                RemovePidFile();
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, ex.Message);
                throw new RadarrStartupException("Unable to migrate DB from nzbdrone.db to {1}. Migrate manually", _appFolderInfo.GetDatabase());
            }
        }
        private void CleanupSqLiteRollbackFiles()
        {
            _diskProvider.GetFiles(_appFolderInfo.AppDataFolder, SearchOption.TopDirectoryOnly)
                         .Where(f => Path.GetFileName(f).StartsWith("nzbdrone.db"))
                         .ToList()
                         .ForEach(_diskProvider.DeleteFile);
        }
        private void RemovePidFile()
        {
            if (OsInfo.IsNotWindows)
            {
                _diskProvider.DeleteFile(Path.Combine(_appFolderInfo.AppDataFolder, "radarr.pid"));
            }
        }
    }
}
