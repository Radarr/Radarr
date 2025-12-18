using System;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace Radarr.Http.Frontend.Mappers
{
    public class StaticResourceMapper : StaticResourceMapperBase
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IConfigFileProvider _configFileProvider;

        public StaticResourceMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, IConfigFileProvider configFileProvider, Logger logger)
            : base(diskProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
            _configFileProvider = configFileProvider;
        }

        public override string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar);

            var basePath = Path.GetFullPath(Path.Combine(_appFolderInfo.StartUpFolder, _configFileProvider.UiFolder));
            var fullPath = Path.GetFullPath(Path.Combine(basePath, path));

            // Prevent path traversal attacks - ensure path stays within UI folder
            if (!fullPath.StartsWith(basePath + Path.DirectorySeparatorChar) &&
                !fullPath.Equals(basePath, StringComparison.Ordinal))
            {
                return null;
            }

            return fullPath;
        }

        public override bool CanHandle(string resourceUrl)
        {
            resourceUrl = resourceUrl.ToLowerInvariant();

            if (resourceUrl.StartsWith("/content/manifest") ||
                resourceUrl.StartsWith("/content/browserconfig"))
            {
                return false;
            }

            return resourceUrl.StartsWith("/content") ||
                   resourceUrl.EndsWith(".js") ||
                   resourceUrl.EndsWith(".map") ||
                   resourceUrl.EndsWith(".css") ||
                   (resourceUrl.EndsWith(".ico") && !resourceUrl.Equals("/favicon.ico")) ||
                   resourceUrl.EndsWith(".swf") ||
                   resourceUrl.EndsWith("oauth.html");
        }
    }
}
