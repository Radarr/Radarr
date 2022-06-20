using System;
using System.IO;
using Microsoft.Extensions.Options;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace Radarr.Http.Frontend.Mappers
{
    public class IndexHtmlMapper : HtmlMapperBase
    {
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileOptions;

        public IndexHtmlMapper(IAppFolderInfo appFolderInfo,
                               IDiskProvider diskProvider,
                               IOptionsMonitor<ConfigFileOptions> configFileOptions,
                               Lazy<ICacheBreakerProvider> cacheBreakProviderFactory,
                               Logger logger)
            : base(diskProvider, cacheBreakProviderFactory, logger)
        {
            _configFileOptions = configFileOptions;

            HtmlPath = Path.Combine(appFolderInfo.StartUpFolder, _uiFolder, "index.html");
            UrlBase = configFileOptions.CurrentValue.UrlBase;
        }

        public override string Map(string resourceUrl)
        {
            return HtmlPath;
        }

        public override bool CanHandle(string resourceUrl)
        {
            resourceUrl = resourceUrl.ToLowerInvariant();

            return !resourceUrl.StartsWith("/content") &&
                   !resourceUrl.StartsWith("/mediacover") &&
                   !resourceUrl.Contains(".") &&
                   !resourceUrl.StartsWith("/login");
        }
    }
}
