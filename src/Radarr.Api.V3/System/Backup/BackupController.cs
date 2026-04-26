using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Crypto;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Backup;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.System.Backup
{
    [V3ApiController("system/backup")]
    public class BackupController : Controller
    {
        private readonly IBackupService _backupService;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;

        private static readonly List<string> ValidExtensions = new() { ".zip", ".db", ".xml" };

        public BackupController(IBackupService backupService,
                            IAppFolderInfo appFolderInfo,
                            IDiskProvider diskProvider)
        {
            _backupService = backupService;
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<List<BackupResource>> GetBackupFiles()
        {
            var backups = _backupService.GetBackups();

            return TypedResults.Ok(backups.Select(b => new BackupResource
                {
                    Id = GetBackupId(b),
                    Name = b.Name,
                    Path = $"/backup/{b.Type.ToString().ToLower()}/{b.Name}",
                    Type = b.Type,
                    Size = b.Size,
                    Time = b.Time
                })
                .OrderByDescending(b => b.Time)
                .ToList());
        }

        [RestDeleteById]
        public Results<Ok<object>, NotFound> DeleteBackup(int id)
        {
            var backup = GetBackup(id);

            if (backup == null)
            {
                return TypedResults.NotFound();
            }

            var path = GetBackupPath(backup);

            if (!_diskProvider.FileExists(path))
            {
                return TypedResults.NotFound();
            }

            _diskProvider.DeleteFile(path);

            return TypedResults.Ok<object>(new { });
        }

        [HttpPost("restore/{id:int}")]
        [Produces("application/json")]
        public Results<Ok<BackupRestoreResource>, NotFound> Restore([FromRoute] int id)
        {
            var backup = GetBackup(id);

            if (backup == null)
            {
                return TypedResults.NotFound();
            }

            var path = GetBackupPath(backup);

            _backupService.Restore(path);

            return TypedResults.Ok(new BackupRestoreResource
            {
                RestartRequired = true
            });
        }

        [HttpPost("restore/upload")]
        [Produces("application/json")]
        [RequestFormLimits(MultipartBodyLengthLimit = 5000000000)]
        public Results<Ok<BackupRestoreResource>, BadRequest<BackupRestoreErrorResource>> RestoreUpload()
        {
            var files = Request.Form.Files;

            if (files.Count == 0)
            {
                throw new BadRequestException("file must be provided");
            }

            var file = files[0];
            var extension = Path.GetExtension(file.FileName);

            if (!ValidExtensions.Contains(extension))
            {
                return TypedResults.BadRequest(new BackupRestoreErrorResource
                {
                    Error = $"Invalid extension, must be one of: {string.Join(", ", ValidExtensions)}"
                });
            }

            var path = Path.Combine(_appFolderInfo.TempFolder, $"radarr_backup_restore{extension}");

            _diskProvider.SaveStream(file.OpenReadStream(), path);
            _backupService.Restore(path);

            // Cleanup restored file
            _diskProvider.DeleteFile(path);

            return TypedResults.Ok(new BackupRestoreResource
            {
                RestartRequired = true
            });
        }

        private string GetBackupPath(NzbDrone.Core.Backup.Backup backup)
        {
            return Path.Combine(_backupService.GetBackupFolder(backup.Type), backup.Name);
        }

        private static int GetBackupId(NzbDrone.Core.Backup.Backup backup)
        {
            return HashConverter.GetHashInt31($"backup-{backup.Type}-{backup.Name}");
        }

        private NzbDrone.Core.Backup.Backup GetBackup(int id)
        {
            return _backupService.GetBackups().SingleOrDefault(b => GetBackupId(b) == id);
        }
    }
}
