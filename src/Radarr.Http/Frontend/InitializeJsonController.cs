using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Analytics;
using NzbDrone.Core.Configuration;

namespace Radarr.Http.Frontend
{
    [Authorize(Policy = "UI")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class InitializeJsonController : Controller
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IAnalyticsService _analyticsService;

        private static string _apiKey;
        private static string _urlBase;
        private string _generatedContent;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        public InitializeJsonController(IConfigFileProvider configFileProvider,
                                      IAnalyticsService analyticsService)
        {
            _configFileProvider = configFileProvider;
            _analyticsService = analyticsService;

            _apiKey = configFileProvider.ApiKey;
            _urlBase = configFileProvider.UrlBase;
        }

        [HttpGet("/initialize.json")]
        public IActionResult Index()
        {
            return Content(GetContent(), "application/json");
        }

        private string GetContent()
        {
            if (RuntimeInfo.IsProduction && _generatedContent != null)
            {
                return _generatedContent;
            }

            var config = new
            {
                apiRoot = $"{_urlBase}/api/v3",
                apiKey = _apiKey,
                release = BuildInfo.Release,
                version = BuildInfo.Version.ToString(),
                instanceName = _configFileProvider.InstanceName,
                theme = _configFileProvider.Theme.ToString(),
                branch = _configFileProvider.Branch.ToLower(),
                analytics = _analyticsService.IsEnabled,
                urlBase = _urlBase,
                isProduction = RuntimeInfo.IsProduction
            };

            _generatedContent = JsonSerializer.Serialize(config, JsonOptions);

            return _generatedContent;
        }
    }
}
