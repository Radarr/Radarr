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

namespace NzbDrone.Core.HealthCheck
{
    public interface IServerSideNotificationService
    {
        public List<HealthCheck> GetServerChecks();
    }

    public class ServerSideNotificationService : IServerSideNotificationService
    {
        private readonly IHttpClient _client;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IHttpRequestBuilderFactory _cloudRequestBuilder;
        private readonly Logger _logger;

        private readonly ICached<List<HealthCheck>> _cache;

        public ServerSideNotificationService(IHttpClient client,
                                             IConfigFileProvider configFileProvider,
                                             IRadarrCloudRequestBuilder cloudRequestBuilder,
                                             ICacheManager cacheManager,
                                             Logger logger)
        {
            _client = client;
            _configFileProvider = configFileProvider;
            _cloudRequestBuilder = cloudRequestBuilder.Services;
            _logger = logger;

            _cache = cacheManager.GetCache<List<HealthCheck>>(GetType());
        }

        public List<HealthCheck> GetServerChecks()
        {
            return _cache.Get("ServerChecks", () => RetrieveServerChecks(), TimeSpan.FromHours(2));
        }

        private List<HealthCheck> RetrieveServerChecks()
        {
            var request = _cloudRequestBuilder.Create()
                                      .Resource("/notification")
                                      .AddQueryParam("version", BuildInfo.Version)
                                      .AddQueryParam("os", OsInfo.Os.ToString().ToLowerInvariant())
                                      .AddQueryParam("arch", RuntimeInformation.OSArchitecture)
                                      .AddQueryParam("runtime", "netcore")
                                      .AddQueryParam("branch", _configFileProvider.Branch)
                                      .Build();
            try
            {
                _logger.Trace("Getting server side health notifications");
                var response = _client.Execute(request);
                var result = Json.Deserialize<List<ServerNotificationResponse>>(response.Content);
                return result.Select(x => new HealthCheck(GetType(), x.Type, x.Message, x.WikiUrl)).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to retrieve server notifications");
                return new List<HealthCheck>();
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
