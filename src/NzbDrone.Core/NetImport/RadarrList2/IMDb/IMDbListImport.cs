using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.NetImport.RadarrList2.IMDbList
{
    public class IMDbListImport : HttpNetImportBase<IMDbListSettings>
    {
        private readonly IHttpRequestBuilderFactory _radarrMetadata;

        public override string Name => "IMDb Lists";

        public override NetImportType ListType => NetImportType.Other;
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public IMDbListImport(IRadarrCloudRequestBuilder requestBuilder,
                              IHttpClient httpClient,
                              INetImportStatusService netImportStatusService,
                              IConfigService configService,
                              IParsingService parsingService,
                              Logger logger)
        : base(httpClient, netImportStatusService, configService, parsingService, logger)
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

                yield return new NetImportDefinition
                {
                    Name = "IMDb Top 250",
                    Enabled = Enabled,
                    EnableAuto = true,
                    ProfileId = 1,
                    Implementation = GetType().Name,
                    Settings = new IMDbListSettings { ListId = "top250" },
                };
                yield return new NetImportDefinition
                {
                    Name = "IMDb Popular Movies",
                    Enabled = Enabled,
                    EnableAuto = true,
                    ProfileId = 1,
                    Implementation = GetType().Name,
                    Settings = new IMDbListSettings { ListId = "popular" },
                };
            }
        }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new IMDbListRequestGenerator()
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
