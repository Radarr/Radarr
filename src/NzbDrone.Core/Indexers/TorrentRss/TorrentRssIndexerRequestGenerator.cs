using System;
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

        public IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
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

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }
}
