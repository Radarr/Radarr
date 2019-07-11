using System.Collections.Generic;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.NetImport.Radarr;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.NetImport.RadarrInstance
{
    public class RadarrInstanceLists : HttpNetImportBase<RadarrInstanceSettings>
    {
        public override string Name => "Radarr Instance";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public RadarrInstanceLists(IHttpClient httpClient, IConfigService configService, IParsingService parsingService,
            Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public override IEnumerable<ProviderDefinition> GetDefaultDefinitions()
        {
                foreach (var def in base.GetDefaultDefinitions())
                {
                    yield return def;
                }
        }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new RadarrInstanceRequestGenerator()
            {
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient
            };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new RadarrInstanceParser(Settings);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "getProfiles")
            {
                return ProxyRequest<Profile>("/api/profile");
            }

            return new { };
        }

        private List<TResource> ProxyRequest<TResource>(string path)
        {
            if (Settings.URL.IsNullOrWhiteSpace() || Settings.APIKey.IsNullOrWhiteSpace())
            {
                return new List<TResource>();
            }

            var baseUrl = $"{Settings.URL.TrimEnd('/')}";
            var request = new HttpRequestBuilder(baseUrl).Resource(path).Accept(HttpAccept.Json)
                .SetHeader("X-Api-Key", Settings.APIKey).Build();
            var response = _httpClient.Get(request);
            var results = JsonConvert.DeserializeObject<List<TResource>>(response.Content);
            return results;
        }
    }
}
