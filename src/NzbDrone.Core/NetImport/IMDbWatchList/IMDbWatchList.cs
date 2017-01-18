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
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.NetImport.IMDbWatchList
{
    public class IMDbWatchList : HttpNetImportBase<IMDbWatchListSettings>
    {
        public override string Name => "IMDbWatchList";
        public override string Link => "http://rss.imdb.com/list/";
        public override int ProfileId => 1;
        public override bool Enabled => true;

        public IMDbWatchList(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        { }

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            return new IMDbWatchListRequestGenerator() { Settings = Settings };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new IMDbWatchListParser(Settings);
        }
    }
}
