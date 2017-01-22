using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.PassThePopcorn;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.NetImport.RSSImport
{
    public class RSSImport : HttpNetImportBase<RSSImportSettings>
    {
        public override string Name => "RSSList";
        public override bool Enabled => true;

        public RSSImport(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        { }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                var config = (RSSImportSettings)new RSSImportSettings();
                config.Link = "https://rss.imdb.com/list/YOURLISTID";

                yield return new NetImportDefinition
                {
                    Name = "IMDb Watchlist",
                    Enabled = config.Validate().IsValid && Enabled,
                    Implementation = GetType().Name,
                    Settings = config
                };
            }
        }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new RSSImportRequestGenerator() { Settings = Settings };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new RSSImportParser(Settings);
        }
    }
}
