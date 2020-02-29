using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists.ReadarrLists
{
    public class ReadarrLists : HttpImportListBase<ReadarrListsSettings>
    {
        public override string Name => "Readarr Lists";

        public override ImportListType ListType => ImportListType.Other;

        public override int PageSize => 10;

        private readonly IMetadataRequestBuilder _requestBuilder;

        public ReadarrLists(IHttpClient httpClient, IImportListStatusService importListStatusService, IConfigService configService, IParsingService parsingService, IMetadataRequestBuilder requestBuilder, Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
            _requestBuilder = requestBuilder;
        }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                yield return GetDefinition("iTunes Top Albums", GetSettings("itunes/album/top"));
                yield return GetDefinition("iTunes New Albums", GetSettings("itunes/album/new"));
                yield return GetDefinition("Apple Music Top Albums", GetSettings("apple-music/album/top"));
                yield return GetDefinition("Apple Music New Albums", GetSettings("apple-music/album/new"));
                yield return GetDefinition("Billboard Top Albums", GetSettings("billboard/album/top"));
                yield return GetDefinition("Billboard Top Artists", GetSettings("billboard/artist/top"));
                yield return GetDefinition("Last.fm Top Artists", GetSettings("lastfm/artist/top"));
            }
        }

        private ImportListDefinition GetDefinition(string name, ReadarrListsSettings settings)
        {
            return new ImportListDefinition
            {
                EnableAutomaticAdd = false,
                Name = name,
                Implementation = GetType().Name,
                Settings = settings
            };
        }

        private ReadarrListsSettings GetSettings(string url)
        {
            var settings = new ReadarrListsSettings { ListId = url };

            return settings;
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new ReadarrListsRequestGenerator(_requestBuilder) { Settings = Settings };
        }

        public override IParseImportListResponse GetParser()
        {
            return new ReadarrListsParser(Settings);
        }
    }
}
