using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.RadarrList2.StevenLu
{
    public class StevenLu2Import : HttpImportListBase<StevenLu2Settings>
    {
        private readonly IHttpRequestBuilderFactory _radarrMetadata;

        public override string Name => "StevenLu List";

        public override ImportListSource ListType => ImportListSource.Other;
        public override bool Enabled => true;
        public override ImportListType EnableAuto => ImportListType.Manual;

        public StevenLu2Import(IRadarrCloudRequestBuilder requestBuilder,
                              IHttpClient httpClient,
                              IImportListStatusService importListStatusService,
                              IConfigService configService,
                              IParsingService parsingService,
                              Logger logger)
        : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
            _radarrMetadata = requestBuilder.RadarrMetadata;
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new StevenLu2RequestGenerator()
            {
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient,
                RequestBuilder = _radarrMetadata
            };
        }

        public override IParseImportListResponse GetParser()
        {
            return new RadarrList2Parser();
        }
    }
}
