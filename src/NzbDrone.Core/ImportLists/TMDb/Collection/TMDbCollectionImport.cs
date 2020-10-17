using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.TMDb.Collection
{
    public class TMDbCollectionImport : TMDbImportListBase<TMDbCollectionSettings>
    {
        public TMDbCollectionImport(IRadarrCloudRequestBuilder requestBuilder,
                                 IHttpClient httpClient,
                                 IImportListStatusService importListStatusService,
                                 IConfigService configService,
                                 IParsingService parsingService,
                                 ISearchForNewMovie searchForNewMovie,
                                 Logger logger)
        : base(requestBuilder, httpClient, importListStatusService, configService, parsingService, searchForNewMovie, logger)
        {
        }

        public override string Name => "TMDb Collection";
        public override bool Enabled => true;
        public override ImportListType EnableAuto => ImportListType.Manual;

        public override IParseImportListResponse GetParser()
        {
            return new TMDbCollectionParser();
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new TMDbCollectionRequestGenerator()
            {
                RequestBuilder = _requestBuilder,
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient
            };
        }
    }
}
