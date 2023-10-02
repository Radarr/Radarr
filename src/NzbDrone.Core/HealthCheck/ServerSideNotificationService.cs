using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.HealthCheck
{
    public class ServerSideNotificationService : HealthCheckBase
    {
        private readonly IHttpClient _client;
        private readonly IRadarrCloudRequestBuilder _cloudRequestBuilder;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        private readonly ICached<HealthCheck> _cache;

        public ServerSideNotificationService(IHttpClient client,
                                             IRadarrCloudRequestBuilder cloudRequestBuilder,
                                             IConfigFileProvider configFileProvider,
                                             ILocalizationService localizationService,
                                             ICacheManager cacheManager,
                                             Logger logger)
            : base(localizationService)
        {
            _client = client;
            _cloudRequestBuilder = cloudRequestBuilder;
            _configFileProvider = configFileProvider;
            _logger = logger;

            _cache = cacheManager.GetCache<HealthCheck>(GetType());
        }

        public override HealthCheck Check()
        {
            return _cache.Get("ServerChecks", RetrieveServerChecks, TimeSpan.FromHours(2));
        }

        private HealthCheck RetrieveServerChecks()
        {
            var request = _cloudRequestBuilder.Services.Create()
                .Resource("/notification")
                .AddQueryParam("version", BuildInfo.Version)
                .AddQueryParam("os", OsInfo.Os.ToString().ToLowerInvariant())
                .AddQueryParam("arch", RuntimeInformation.OSArchitecture)
                .AddQueryParam("runtime", "netcore")
                .AddQueryParam("branch", _configFileProvider.Branch)
                .Build();

            try
            {
                _logger.Trace("Getting notifications");

                var response = _client.Execute(request);
                var result = Json.Deserialize<List<ServerNotificationResponse>>(response.Content);

                var checks = result.Select(x => new HealthCheck(GetType(), x.Type, x.Message, x.WikiUrl)).ToList();

                // Only one health check is supported, services returns an ordered list, so use the first one
                return checks.FirstOrDefault() ?? new HealthCheck(GetType());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to retrieve notifications");

                return new HealthCheck(GetType());
            }
        }
    }

    public class ServerNotificationResponse
    {
        public HealthCheckResult Type { get; set; }
        public string Message { get; set; }
        public string WikiUrl { get; set; }
    }
}
