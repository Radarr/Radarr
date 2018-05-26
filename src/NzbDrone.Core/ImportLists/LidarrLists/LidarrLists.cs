using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists.LidarrLists
{
    public class LidarrLists : HttpImportListBase<LidarrListsSettings>
    {
        public override string Name => "Lidarr Lists";

        public override int PageSize => 10;

        public LidarrLists(IHttpClient httpClient, IImportListStatusService importListStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {

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
                yield return GetDefinition("Last.fm Top Albums", GetSettings("lastfm/album/top"));
                yield return GetDefinition("Last.fm Top Artists", GetSettings("lastfm/artist/top"));
            }
        }

        private ImportListDefinition GetDefinition(string name, LidarrListsSettings settings)
        {
            return new ImportListDefinition
            {
                EnableAutomaticAdd = false,
                Name = name,
                Implementation = GetType().Name,
                Settings = settings
            };
        }

        private LidarrListsSettings GetSettings(string url)
        {
            var settings = new LidarrListsSettings { ListId = url };

            return settings;
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new LidarrListsRequestGenerator { Settings = Settings, PageSize = PageSize };
        }

        public override IParseImportListResponse GetParser()
        {
            return new LidarrListsParser(Settings);
        }

    }
}
