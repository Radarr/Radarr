using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.NetImport.TMDb.Collection
{
    public class TMDbCollectionImport : TMDbNetImportBase<TMDbCollectionSettings>
    {
        public TMDbCollectionImport(IRadarrCloudRequestBuilder requestBuilder,
                                 IHttpClient httpClient,
                                 IConfigService configService,
                                 IParsingService parsingService,
                                 ISearchForNewMovie searchForNewMovie,
                                 Logger logger)
        : base(requestBuilder, httpClient, configService, parsingService, searchForNewMovie, logger)
        {
        }

        public override string Name => "TMDb Collection";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override IParseNetImportResponse GetParser()
        {
            return new TMDbCollectionParser();
        }

        public override INetImportRequestGenerator GetRequestGenerator()
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
