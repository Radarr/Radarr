using System.IO;
using Microsoft.Extensions.Options;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace Radarr.Http.Frontend.Mappers
{
    public class FaviconMapper : StaticResourceMapperBase
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileOptions;

        public FaviconMapper(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, IOptionsMonitor<ConfigFileOptions> configFileOptions, Logger logger)
            : base(diskProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
            _configFileOptions = configFileOptions;
        }

        public override string Map(string resourceUrl)
        {
            var fileName = "favicon.ico";

            if (BuildInfo.IsDebug)
            {
                fileName = "favicon-debug.ico";
            }

            var path = Path.Combine("Content", "Images", "Icons", fileName);

            return Path.Combine(_appFolderInfo.StartUpFolder, _uiFolder, path);
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.Equals("/favicon.ico");
        }
    }
}
