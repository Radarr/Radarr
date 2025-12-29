using System;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace Radarr.Http.Frontend.Mappers
{
    public class MediaCoverMapper : StaticResourceMapperBase
    {
        private static readonly Regex RegexResizedImage = new Regex(@"-\d+\.jpg($|\?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IDiskProvider _diskProvider;

        public MediaCoverMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
            _diskProvider = diskProvider;
        }

        public override string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar);

            var basePath = Path.GetFullPath(_appFolderInfo.GetAppDataPath());
            var resourcePath = Path.GetFullPath(Path.Combine(basePath, path));

            // Prevent path traversal attacks - ensure path stays within AppData folder
            if (!resourcePath.StartsWith(basePath + Path.DirectorySeparatorChar) &&
                !resourcePath.Equals(basePath, StringComparison.Ordinal))
            {
                return null;
            }

            if (!_diskProvider.FileExists(resourcePath) || _diskProvider.GetFileSize(resourcePath) == 0)
            {
                var baseResourcePath = RegexResizedImage.Replace(resourcePath, ".jpg$1");
                if (baseResourcePath != resourcePath)
                {
                    return baseResourcePath;
                }
            }

            return resourcePath;
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/MediaCover/", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
