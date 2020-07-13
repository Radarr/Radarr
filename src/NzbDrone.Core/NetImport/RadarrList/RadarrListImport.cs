using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.NetImport.RadarrList
{
    public class RadarrListImport : HttpNetImportBase<RadarrListSettings>
    {
        public override string Name => "Custom Lists";

        public override NetImportType ListType => NetImportType.Advanced;
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public RadarrListImport(IHttpClient httpClient,
            INetImportStatusService netImportStatusService,
            IConfigService configService,
            IParsingService parsingService,
            Logger logger)
            : base(httpClient, netImportStatusService, configService, parsingService, logger)
        {
        }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                foreach (var def in base.DefaultDefinitions)
                {
                    yield return def;
                }
            }
        }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new RadarrListRequestGenerator()
            {
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient
            };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new RadarrListParser();
        }
    }
}
