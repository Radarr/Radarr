using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.TMDb.Keyword
{
    public class TMDbKeywordImport : TMDbImportListBase<TMDbKeywordSettings>
    {
        public override string Name => "TMDb Keyword";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public TMDbKeywordImport(IRadarrCloudRequestBuilder requestBuilder,
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
            return new TMDbKeywordParser();
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new TMDbKeywordRequestGenerator
            {
                RequestBuilder = _requestBuilder,
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient
            };
        }
    }
}
