using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists.RadarrList
{
    public class RadarrListImport : HttpImportListBase<RadarrListSettings>
    {
        public override string Name => "Custom Lists";

        public override ImportListSource ListType => ImportListSource.Advanced;
        public override bool Enabled => true;
        public override ImportListType EnableAuto => ImportListType.Manual;

        public RadarrListImport(IHttpClient httpClient,
            IImportListStatusService importListStatusService,
            IConfigService configService,
            IParsingService parsingService,
            Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
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

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new RadarrListRequestGenerator()
            {
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient
            };
        }

        public override IParseImportListResponse GetParser()
        {
            return new RadarrListParser();
        }
    }
}
