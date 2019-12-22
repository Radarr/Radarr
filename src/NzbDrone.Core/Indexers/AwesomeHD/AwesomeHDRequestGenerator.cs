using System;
using System.Collections.Generic;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.AwesomeHD
{
    public class AwesomeHDRequestGenerator : IIndexerRequestGenerator
    {
        public AwesomeHDSettings Settings { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();
            pageableRequests.Add(GetRequest(null));
            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            pageableRequests.Add(GetRequest(searchCriteria.Movie.ImdbId));
            return pageableRequests;
        }

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }

        private IEnumerable<IndexerRequest> GetRequest(string searchParameters)
        {
            if (searchParameters != null)
            {
                yield return new IndexerRequest($"{Settings.BaseUrl.Trim().TrimEnd('/')}/searchapi.php?action=imdbsearch&passkey={Settings.Passkey.Trim()}&imdb={searchParameters}", HttpAccept.Rss);
            }
            else
            {
                yield return new IndexerRequest($"{Settings.BaseUrl.Trim().TrimEnd('/')}/searchapi.php?action=latestmovies&passkey={Settings.Passkey.Trim()}", HttpAccept.Rss);
            }
        }
    }
}
