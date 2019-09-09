using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NLog;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.Waffles
{
    public class Waffles : HttpIndexerBase<WafflesSettings>
    {
        public override string Name => "Waffles";

        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override int PageSize => 15;

        public Waffles(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {

        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new WafflesRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new WafflesRssParser() { ParseSizeInDescription = true, ParseSeedersInDescription = true };
        }
    }
}