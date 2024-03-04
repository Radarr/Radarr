using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists.Rss.Imdb
{
    public class ImdbRssImport : RssImportBase<ImdbRssImportSettings>
    {
        public override string Name => "IMDb RSS";
        public override ImportListType ListType => ImportListType.Advanced;

        public ImdbRssImport(IHttpClient httpClient,
                             IImportListStatusService importListStatusService,
                             IConfigService configService,
                             IParsingService parsingService,
                             Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
        }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                yield return new ImportListDefinition
                {
                    Name = "IMDb RSS List",
                    Enabled = Enabled,
                    EnableAuto = true,
                    ProfileId = 1,
                    Implementation = GetType().Name,
                    Settings = new ImdbRssImportSettings { Url = "https://rss.imdb.com/list/YOURLISTID" },
                };
                yield return new ImportListDefinition
                {
                    Name = "IMDb RSS Watchlist",
                    Enabled = Enabled,
                    EnableAuto = true,
                    ProfileId = 1,
                    Implementation = GetType().Name,
                    Settings = new ImdbRssImportSettings { Url = "https://rss.imdb.com/user/IMDBUSERID/watchlist" },
                };
            }
        }

        public override IParseImportListResponse GetParser()
        {
            return new ImdbRssImportParser(_logger);
        }
    }
}
