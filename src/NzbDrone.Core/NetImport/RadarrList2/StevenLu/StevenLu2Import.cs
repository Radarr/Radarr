using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.NetImport.RadarrList2.StevenLu
{
    public class StevenLu2Import : HttpNetImportBase<StevenLu2Settings>
    {
        private readonly IHttpRequestBuilderFactory _radarrMetadata;

        public override string Name => "StevenLu List";

        public override NetImportType ListType => NetImportType.Other;
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public StevenLu2Import(IRadarrCloudRequestBuilder requestBuilder,
                              IHttpClient httpClient,
                              INetImportStatusService netImportStatusService,
                              IConfigService configService,
                              IParsingService parsingService,
                              Logger logger)
        : base(httpClient, netImportStatusService, configService, parsingService, logger)
        {
            _radarrMetadata = requestBuilder.RadarrMetadata;
        }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new StevenLu2RequestGenerator()
            {
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient,
                RequestBuilder = _radarrMetadata
            };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new RadarrList2Parser();
        }
    }
}
