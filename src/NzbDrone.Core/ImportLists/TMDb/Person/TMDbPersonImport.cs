using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.TMDb.Person
{
    public class TMDbPersonImport : TMDbImportListBase<TMDbPersonSettings>
    {
        public override string Name => "TMDb Person";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public TMDbPersonImport(IRadarrCloudRequestBuilder requestBuilder,
                                 IHttpClient httpClient,
                                 IImportListStatusService importListStatusService,
                                 IConfigService configService,
                                 IParsingService parsingService,
                                 ICacheManager cacheManager,
                                 Logger logger)
        : base(requestBuilder, httpClient, importListStatusService, configService, parsingService, cacheManager, logger)
        {
        }

        public override IParseImportListResponse GetParser()
        {
            return new TMDbPersonParser(Settings);
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new TMDbPersonRequestGenerator
            {
                RequestBuilder = _requestBuilder,
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient
            };
        }
    }
}
