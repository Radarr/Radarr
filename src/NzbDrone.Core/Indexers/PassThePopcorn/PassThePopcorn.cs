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

        private readonly ICached<Dictionary<string, string>> _authCookieCache;
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public PassThePopcorn(IHttpClient httpClient, ICacheManager cacheManager, IIndexerStatusService indexerStatusService,
            IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _authCookieCache = cacheManager.GetCache<Dictionary<string, string>>(GetType(), "authCookies");
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new PassThePopcornRequestGenerator()
            {
                Settings = Settings,
                HttpClient = _httpClient,
                Logger = _logger,
                AuthCookieCache = _authCookieCache
            };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new PassThePopcornParser(Settings);
        }
    }
}
