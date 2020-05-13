using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers
{
    public class RssIndexerRequestGenerator : IIndexerRequestGenerator
    {
        private readonly string _baseUrl;

        public RssIndexerRequestGenerator(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(new[] { new IndexerRequest(_baseUrl, HttpAccept.Rss) });

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
    }
}
