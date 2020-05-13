using System.Collections.Generic;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Torrentleech
{
    public class TorrentleechRequestGenerator : IIndexerRequestGenerator
    {
        public TorrentleechSettings Settings { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRssRequests(null));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(BookSearchCriteria searchCriteria)
        {
            throw new System.NotImplementedException();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AuthorSearchCriteria searchCriteria)
        {
            throw new System.NotImplementedException();
        }

        private IEnumerable<IndexerRequest> GetRssRequests(string searchParameters)
        {
            yield return new IndexerRequest(string.Format("{0}/{1}{2}", Settings.BaseUrl.Trim().TrimEnd('/'), Settings.ApiKey, searchParameters), HttpAccept.Rss);
        }
    }
}
