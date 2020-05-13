using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.TorrentRss
{
    public class TorrentRssIndexerRequestGenerator : IIndexerRequestGenerator
    {
        public TorrentRssIndexerSettings Settings { get; set; }

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
            var request = new IndexerRequest(Settings.BaseUrl.Trim().TrimEnd('/'), HttpAccept.Rss);

            if (Settings.Cookie.IsNotNullOrWhiteSpace())
            {
                foreach (var cookie in HttpHeader.ParseCookies(Settings.Cookie))
                {
                    request.HttpRequest.Cookies[cookie.Key] = cookie.Value;
                }
            }

            yield return request;
        }
    }
}
