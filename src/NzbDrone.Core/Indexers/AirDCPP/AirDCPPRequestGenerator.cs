using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.Indexers.AirDCPP
{
    public class AirDCPPRequestGenerator : IIndexerRequestGenerator
    {
        private readonly IHttpClient _httpClient;
        private readonly IAirDCPPProxy _airDCPPProxy;

        public AirDCPPRequestGenerator(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _airDCPPProxy = new AirDCPPProxy(_httpClient, logger);
        }

        public AirDCPPSettings Settings { get; set; }

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRequest("a"));

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (searchCriteria?.SceneTitles != null)
            {
                searchCriteria.SceneTitles.ForEach(sceneTitle =>
                    pageableRequests.Add(GetRequest(FileNameBuilder.CleanFileName(sceneTitle))));
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetRequest(string searchName)
        {
            var request = _airDCPPProxy.PerformSearch(Settings, searchName);
            yield return new IndexerRequest(request);
        }
    }
}
