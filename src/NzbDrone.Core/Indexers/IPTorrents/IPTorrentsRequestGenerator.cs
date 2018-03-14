using System;
using System.Collections.Generic;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.IPTorrents
{
    public class IPTorrentsRequestGenerator : IIndexerRequestGenerator
    {
        public IPTorrentsSettings Settings { get; set; }
        
        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRssRequests());

            return pageableRequests;
        }
        
        public IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        private IEnumerable<IndexerRequest> GetRssRequests()
        {
            yield return new IndexerRequest(Settings.BaseUrl, HttpAccept.Rss);
        }
    }
}
