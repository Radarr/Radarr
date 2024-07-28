using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.TMDb.List
{
    public class TMDbListImport : TMDbImportListBase<TMDbListSettings>
    {
        public override string Name => "TMDb List";
        public override bool Enabled => true;
        public override bool EnableAuto => false;
        public override int PageSize => 1;

        public TMDbListImport(IRadarrCloudRequestBuilder requestBuilder,
                                 IHttpClient httpClient,
                                 IImportListStatusService importListStatusService,
                                 IConfigService configService,
                                 IParsingService parsingService,
                                 ISearchForNewMovie searchForNewMovie,
                                 Logger logger)
        : base(requestBuilder, httpClient, importListStatusService, configService, parsingService, searchForNewMovie, logger)
        {
        }

        public override IParseImportListResponse GetParser()
        {
            return new TMDbListParser();
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new TMDbListRequestGenerator
            {
                RequestBuilder = _requestBuilder,
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient
            };
        }
    }
}
