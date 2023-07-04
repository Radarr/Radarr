using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists.RSSImport
{
    public class RSSImport : HttpImportListBase<RSSImportSettings>
    {
        public override string Name => "RSS List";

        public override ImportListType ListType => ImportListType.Advanced;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(12);
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public RSSImport(IHttpClient httpClient, IImportListStatusService importListStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
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
                    Name = "IMDb List",
                    Enabled = Enabled,
                    EnableAuto = true,
                    ProfileId = 1,
                    Implementation = GetType().Name,
                    Settings = new RSSImportSettings { Link = "https://rss.imdb.com/list/YOURLISTID" },
                };
                yield return new ImportListDefinition
                {
                    Name = "IMDb Watchlist",
                    Enabled = Enabled,
                    EnableAuto = true,
                    ProfileId = 1,
                    Implementation = GetType().Name,
                    Settings = new RSSImportSettings { Link = "https://rss.imdb.com/user/IMDBUSERID/watchlist" },
                };
            }
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new RSSImportRequestGenerator() { Settings = Settings };
        }

        public override IParseImportListResponse GetParser()
        {
            return new RSSImportParser(Settings, _logger);
        }
    }
}
