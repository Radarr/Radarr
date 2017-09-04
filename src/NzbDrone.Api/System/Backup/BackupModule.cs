using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Core.Backup;
using Lidarr.Http;

namespace NzbDrone.Api.System.Backup
{
    public class BackupModule : LidarrRestModule<BackupResource>
    {
        private readonly IBackupService _backupService;

        public BackupModule(IBackupService backupService) : base("system/backup")
        {
            _backupService = backupService;
            GetResourceAll = GetBackupFiles;
        }

        public List<BackupResource> GetBackupFiles()
        {
            var backups = _backupService.GetBackups();

            return backups.Select(b => new BackupResource
                                       {
                                           Id = b.Name.GetHashCode(),
                                           Name = Path.GetFileName(b.Name),
                                           Path = b.Name,
                                           Type = b.Type,
                                           Time = b.Time
                                       }).ToList();
        }
    }
}
