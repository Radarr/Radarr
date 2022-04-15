using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists.RadarrList2.IMDbList
{
    public class IMDbListImport : HttpImportListBase<IMDbListSettings>
    {
        private readonly IHttpRequestBuilderFactory _radarrMetadata;

        public override string Name => "IMDb Lists";

        public override ImportListType ListType => ImportListType.Other;
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public IMDbListImport(IRadarrCloudRequestBuilder requestBuilder,
                              IHttpClient httpClient,
                              IImportListStatusService importListStatusService,
                              IConfigService configService,
                              IParsingService parsingService,
                              Logger logger)
        : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
            _radarrMetadata = requestBuilder.RadarrMetadata;
        }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                foreach (var def in base.DefaultDefinitions)
                {
                    yield return def;
                }

                yield return new ImportListDefinition
                {
                    Name = "IMDb Top 250",
                    Enabled = Enabled,
                    EnableAuto = true,
                    QualityProfileIds = new List<int> { 1 },
                    Implementation = GetType().Name,
                    Settings = new IMDbListSettings { ListId = "top250" },
                };
                yield return new ImportListDefinition
                {
                    Name = "IMDb Popular Movies",
                    Enabled = Enabled,
                    EnableAuto = true,
                    QualityProfileIds = new List<int> { 1 },
                    Implementation = GetType().Name,
                    Settings = new IMDbListSettings { ListId = "popular" },
                };
            }
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new IMDbListRequestGenerator()
            {
                Settings = Settings,
                Logger = _logger,
                HttpClient = _httpClient,
                RequestBuilder = _radarrMetadata
            };
        }

        public override IParseImportListResponse GetParser()
        {
            return new IMDbListParser(Settings);
        }
    }
}
