using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.NetImport.TMDb.Person
{
    public class TMDbPersonImport : TMDbNetImportBase<TMDbPersonSettings>
    {
        public TMDbPersonImport(IRadarrCloudRequestBuilder requestBuilder,
                                 IHttpClient httpClient,
                                 IConfigService configService,
                                 IParsingService parsingService,
                                 ISearchForNewMovie searchForNewMovie,
                                 Logger logger)
        : base(requestBuilder, httpClient, configService, parsingService, searchForNewMovie, logger)
        {
        }

        public override string Name => "TMDb Person";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public override IParseNetImportResponse GetParser()
        {
            return new TMDbPersonParser(Settings);
        }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new TMDbPersonRequestGenerator()
            {
                RequestBuilder = _requestBuilder,
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient
            };
        }
    }
}
