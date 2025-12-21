using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Backup;

namespace Radarr.Http.Frontend.Mappers
{
    public class BackupFileMapper : StaticResourceMapperBase
    {
        private readonly IBackupService _backupService;

        public BackupFileMapper(IBackupService backupService, IDiskProvider diskProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _backupService = backupService;
        }

        public override string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace("/backup/", "").Replace('/', Path.DirectorySeparatorChar);

            var basePath = Path.GetFullPath(_backupService.GetBackupFolder());
            var filePath = Path.GetFullPath(Path.Combine(basePath, path));

            // Prevent path traversal - ensure path stays within backup folder
            if (!filePath.StartsWith(basePath + Path.DirectorySeparatorChar) &&
                !filePath.Equals(basePath, System.StringComparison.Ordinal))
            {
                return null;
            }

            return filePath;
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/backup/") && BackupService.BackupFileRegex.IsMatch(resourceUrl);
        }
    }
}
