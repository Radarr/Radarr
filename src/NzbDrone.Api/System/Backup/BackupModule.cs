﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Core.Backup;

namespace NzbDrone.Api.System.Backup
{
    public class BackupModule : NzbDroneRestModule<BackupResource>
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
                                           Name = b.Name,
                                           Path = $"/backup/{b.Type.ToString().ToLower()}/{b.Name}",
                                           Type = b.Type,
                                           Time = b.Time
                                       }).ToList();
        }
    }
}
