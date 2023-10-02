using System;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.PassThePopcorn
{
    public class PassThePopcornRequestGenerator : IIndexerRequestGenerator
    {
        private readonly PassThePopcornSettings _settings;

        public PassThePopcornRequestGenerator(PassThePopcornSettings settings)
        {
            _settings = settings;
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRequest(null));

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (searchCriteria.Movie.MovieMetadata.Value.ImdbId.IsNotNullOrWhiteSpace())
            {
                pageableRequests.Add(GetRequest(searchCriteria.Movie.MovieMetadata.Value.ImdbId));
            }
            else if (searchCriteria.Movie.Year > 0)
            {
                foreach (var queryTitle in searchCriteria.CleanSceneTitles)
                {
                    pageableRequests.Add(GetRequest($"{queryTitle}&year={searchCriteria.Movie.Year}"));
                }
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetRequest(string searchParameters)
        {
            var request =
                new IndexerRequest(
                    $"{_settings.BaseUrl.Trim().TrimEnd('/')}/torrents.php?action=advanced&json=noredirect&grouping=0&searchstr={searchParameters}",
                    HttpAccept.Json);

            request.HttpRequest.Headers.Add("ApiUser", _settings.APIUser);
            request.HttpRequest.Headers.Add("ApiKey", _settings.APIKey);

            yield return request;
        }

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }
}
