using System.IO;
using Microsoft.Extensions.Options;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace Radarr.Http.Frontend.Mappers
{
    public class BrowserConfig : StaticResourceMapperBase
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileOptions;

        public BrowserConfig(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, IOptionsMonitor<ConfigFileOptions> configFileOptions, Logger logger)
            : base(diskProvider, logger)
        {
            _appFolderInfo = appFolderInfo;
            _configFileOptions = configFileOptions;
        }

        public override string Map(string resourceUrl)
        {
            var path = resourceUrl.Replace('/', Path.DirectorySeparatorChar);
            path = path.Trim(Path.DirectorySeparatorChar);

            return Path.ChangeExtension(Path.Combine(_appFolderInfo.StartUpFolder, _uiFolder, path), "xml");
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/content/images/icons/browserconfig");
        }
    }
}
