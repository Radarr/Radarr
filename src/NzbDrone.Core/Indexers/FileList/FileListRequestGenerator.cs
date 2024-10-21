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

            if (searchCriteria.Movie.MovieMetadata.Value.ImdbId.IsNotNullOrWhiteSpace())
            {
                pageableRequests.Add(GetRequest("search-torrents", $"&type=imdb&query={searchCriteria.Movie.MovieMetadata.Value.ImdbId}"));
            }
            else if (searchCriteria.Movie.Year > 0)
            {
                foreach (var queryTitle in searchCriteria.CleanSceneTitles)
                {
                    var titleYearSearchQuery = $"{queryTitle}+{searchCriteria.Movie.Year}";
                    pageableRequests.Add(GetRequest("search-torrents", $"&type=name&query={titleYearSearchQuery.Trim()}"));
                }
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetRequest(string searchType, string parameters)
        {
            var categoriesQuery = string.Join(",", Settings.Categories.Distinct());

            var baseUrl = $"{Settings.BaseUrl.TrimEnd('/')}/api.php?action={searchType}&category={categoriesQuery}{parameters}";

            var request = new IndexerRequest(baseUrl, HttpAccept.Json);
            request.HttpRequest.Credentials = new BasicNetworkCredential(Settings.Username.Trim(), Settings.Passkey.Trim());

            yield return request;
        }
    }
}
