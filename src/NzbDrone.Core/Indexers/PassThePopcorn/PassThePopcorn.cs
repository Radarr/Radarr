using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.PassThePopcorn
{
    public class PassThePopcorn : HttpIndexerBase<PassThePopcornSettings>
    {
        public override string Name => "PassThePopcorn";
        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override bool SupportsRss => true;
        public override bool SupportsSearch => true;
        public override int PageSize => 50;

        private readonly IHttpClient _httpClient;
        private readonly IIndexerStatusService _indexerStatusService;
        private readonly Logger _logger;

        public PassThePopcorn(IHttpClient httpClient, ICacheManager cacheManager, IIndexerStatusService indexerStatusService,
            IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _indexerStatusService = indexerStatusService;
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new PassThePopcornRequestGenerator()
            {
                Settings = Settings,
                HttpClient = _httpClient,
                Logger = _logger,
            };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new PassThePopcornParser(Settings);
        }
        
        /*protected override IndexerResponse FetchIndexerResponse(IndexerRequest request)
        {
            _logger.Debug("Downloading Feed " + request.HttpRequest.ToString(false));

            if (request.HttpRequest.RateLimit < RateLimit)
            {
                request.HttpRequest.RateLimit = RateLimit;
            }

            //Potentially dangerous though if ptp moves domains!
            request.HttpRequest.AllowAutoRedirect = false;

            return new IndexerResponse(request, _httpClient.Execute(request.HttpRequest));
        }*/
    }
}
