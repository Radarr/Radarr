using System.Collections.Generic;
using System.IO;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using Radarr.Http;

namespace Radarr.Api.V3.Logs
{
    [V3ApiController("log/file")]
    public class LogFileController : LogFileControllerBase
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;

        public LogFileController(IAppFolderInfo appFolderInfo,
                             IDiskProvider diskProvider,
                             IConfigFileProvider configFileProvider)
            : base(diskProvider, configFileProvider, "")
        {
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
        }

        protected override IEnumerable<string> GetLogFiles()
        {
            return _diskProvider.GetFiles(_appFolderInfo.GetLogFolder(), false);
        }

        protected override string GetLogFilePath(string filename)
        {
            var logFolder = Path.GetFullPath(_appFolderInfo.GetLogFolder());
            var filePath = Path.GetFullPath(Path.Combine(logFolder, filename));

            // Prevent path traversal - ensure path stays within log folder
            if (!filePath.StartsWith(logFolder + Path.DirectorySeparatorChar) &&
                !filePath.Equals(logFolder, global::System.StringComparison.Ordinal))
            {
                return null;
            }

            return filePath;
        }

        protected override string DownloadUrlRoot
        {
            get
            {
                return "logfile";
            }
        }
    }
}
