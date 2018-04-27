using System;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Analytics;
using NzbDrone.Core.Configuration;

namespace Lidarr.Http.Frontend.Mappers
{
    public class IndexHtmlMapper : HtmlMapperBase
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IAnalyticsService _analyticsService;

        private static string API_KEY;

        public IndexHtmlMapper(IAppFolderInfo appFolderInfo,
                               IDiskProvider diskProvider,
                               IConfigFileProvider configFileProvider,
                               IAnalyticsService analyticsService,
                               Func<ICacheBreakerProvider> cacheBreakProviderFactory,
                               Logger logger)
            : base(diskProvider, cacheBreakProviderFactory, logger)
        {
            _configFileProvider = configFileProvider;
            _analyticsService = analyticsService;

            HtmlPath = Path.Combine(appFolderInfo.StartUpFolder, _configFileProvider.UiFolder, "index.html");
            UrlBase = configFileProvider.UrlBase;

            API_KEY = configFileProvider.ApiKey;
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

        protected override string ReplaceText(string text)
        {
            text = text.Replace("API_ROOT", UrlBase + "/api/v1");
            text = text.Replace("API_KEY", API_KEY);
            text = text.Replace("RELEASE", BuildInfo.Release);
            text = text.Replace("APP_VERSION", BuildInfo.Version.ToString());
            text = text.Replace("APP_BRANCH", _configFileProvider.Branch.ToLower());
            text = text.Replace("APP_ANALYTICS", _analyticsService.IsEnabled.ToString().ToLowerInvariant());
            text = text.Replace("URL_BASE", UrlBase);
            text = text.Replace("IS_PRODUCTION", RuntimeInfo.IsProduction.ToString().ToLowerInvariant());

            return text;
        }
    }
}
