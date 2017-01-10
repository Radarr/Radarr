using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Wombles
{
    public class Wombles : HttpIndexerBase<NullConfig>
    {
        public override string Name => "Womble's";

        public override DownloadProtocol Protocol => DownloadProtocol.Usenet;
        public override bool SupportsSearch => false;

        public override IParseIndexerResponse GetParser()
        {
            return new WomblesRssParser();
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new RssIndexerRequestGenerator("http://newshost.co.za/rss/?sec=Movies&fr=false");
        }

        public Wombles(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {

        }
    }
}