using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.FileList
{
    public class FileListRequestGenerator : IIndexerRequestGenerator
    {
        public FileListSettings Settings { get; set; }
        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();
            pageableRequests.Add(GetRequest("latest-torrents", ""));
            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (searchCriteria.Movie.ImdbId.IsNotNullOrWhiteSpace())
            {
                pageableRequests.Add(GetRequest("search-torrents", string.Format("&type=imdb&query={0}", searchCriteria.Movie.ImdbId)));
            }
            else
            {
                foreach (var queryTitle in searchCriteria.QueryTitles)
                {
                    var titleYearSearchQuery = string.Format("{0}+{1}", queryTitle, searchCriteria.Movie.Year);
                    pageableRequests.Add(GetRequest("search-torrents", string.Format("&type=name&query={0}", titleYearSearchQuery.Trim())));
                }
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetRequest(string searchType, string parameters)
        {
            var categoriesQuery = string.Join(",", Settings.Categories.Distinct());

            var baseUrl = string.Format("{0}/api.php?action={1}&category={2}&username={3}&passkey={4}{5}", Settings.BaseUrl.TrimEnd('/'), searchType, categoriesQuery, Settings.Username.Trim(), Settings.Passkey.Trim(), parameters);

            yield return new IndexerRequest(baseUrl, HttpAccept.Json);
        }
    }
}
