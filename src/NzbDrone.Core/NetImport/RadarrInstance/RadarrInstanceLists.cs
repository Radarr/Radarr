using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.NetImport.Radarr;
using NzbDrone.Core.Parser;
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
            return new RadarrInstanceParser();
        }
    }
}
