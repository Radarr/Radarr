using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Rss.Plex
{
    public class PlexRssImport : RssImportBase<PlexRssImportSettings>
    {
        public override string Name => "Plex Watchlist RSS";
        public override ImportListType ListType => ImportListType.Plex;

        public PlexRssImport(IHttpClient httpClient,
            IImportListStatusService importListStatusService,
            IConfigService configService,
            IParsingService parsingService,
            Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override IParseImportListResponse GetParser()
        {
            return new PlexRssImportParser(_logger);
        }
    }
}
