using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.NetImport.RSSImport
{
    public class RSSImport : HttpNetImportBase<RSSImportSettings>
    {
        public override string Name => "RSS List";

        public override NetImportType ListType => NetImportType.Advanced;
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        public RSSImport(IHttpClient httpClient, INetImportStatusService netImportStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, netImportStatusService, configService, parsingService, logger)
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

                yield return new NetImportDefinition
                {
                    Name = "IMDb List",
                    Enabled = Enabled,
                    EnableAuto = true,
                    ProfileId = 1,
                    Implementation = GetType().Name,
                    Settings = new RSSImportSettings { Link = "https://rss.imdb.com/list/YOURLISTID" },
                };
                yield return new NetImportDefinition
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

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new RSSImportRequestGenerator() { Settings = Settings };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new RSSImportParser(Settings, _logger);
        }
    }
}
