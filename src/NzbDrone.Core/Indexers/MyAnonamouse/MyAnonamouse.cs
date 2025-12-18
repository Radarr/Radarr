using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaTypes;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.MyAnonamouse
{
    public class MyAnonamouse : HttpIndexerBase<MyAnonamouseSettings>
    {
        public override string Name => "MyAnonamouse";
        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override bool SupportsRss => true;
        public override bool SupportsSearch => true;
        public override int PageSize => 100;

        public override IEnumerable<MediaType> SupportedMediaTypes => new[] { MediaType.Book, MediaType.Audiobook };

        public MyAnonamouse(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new MyAnonamouseRequestGenerator(Settings, _logger);
        }

        public override IParseIndexerResponse GetParser()
        {
            return new MyAnonamouseParser(Settings);
        }
    }
}
