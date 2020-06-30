using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabRequestGenerator : IIndexerRequestGenerator
    {
        private readonly INewznabCapabilitiesProvider _capabilitiesProvider;
        public int MaxPages { get; set; }
        public int PageSize { get; set; }
        public NewznabSettings Settings { get; set; }

        public NewznabRequestGenerator(INewznabCapabilitiesProvider capabilitiesProvider)
        {
            _capabilitiesProvider = capabilitiesProvider;

            MaxPages = 30;
            PageSize = 100;
        }

        private bool SupportsSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedSearchParameters != null &&
                       capabilities.SupportedSearchParameters.Contains("q");
            }
        }

        private bool SupportsBookSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedBookSearchParameters != null &&
                       capabilities.SupportedBookSearchParameters.Contains("q") &&
                       capabilities.SupportedBookSearchParameters.Contains("author") &&
                       capabilities.SupportedBookSearchParameters.Contains("title");
            }
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

            if (capabilities.SupportedBookSearchParameters != null)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories, "book", ""));
            }
            else if (capabilities.SupportedSearchParameters != null)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories, "search", ""));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(BookSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (SupportsBookSearch)
            {
                AddBookPageableRequests(pageableRequests,
                    searchCriteria,
                    NewsnabifyTitle($"&author={searchCriteria.AuthorQuery}&title={searchCriteria.BookQuery}"));
            }

            if (SupportsSearch)
            {
                pageableRequests.AddTier();

/*                pageableRequests.Add(GetPagedRequests(MaxPages,
                    Settings.Categories,
                    "search",
                    NewsnabifyTitle($"&q={searchCriteria.BookIsbn}")));

                pageableRequests.AddTier();*/

                pageableRequests.Add(GetPagedRequests(MaxPages,
                    Settings.Categories,
                    "search",
                    NewsnabifyTitle($"&q={searchCriteria.BookQuery}+{searchCriteria.AuthorQuery}")));

                pageableRequests.Add(GetPagedRequests(MaxPages,
                    Settings.Categories,
                    "search",
                    NewsnabifyTitle($"&q={searchCriteria.AuthorQuery}+{searchCriteria.BookQuery}")));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AuthorSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (SupportsBookSearch)
            {
                AddBookPageableRequests(pageableRequests,
                    searchCriteria,
                    NewsnabifyTitle($"&author={searchCriteria.AuthorQuery}"));
            }

            if (SupportsSearch)
            {
                pageableRequests.AddTier();

                pageableRequests.Add(GetPagedRequests(MaxPages,
                    Settings.Categories,
                    "search",
                    NewsnabifyTitle($"&q={searchCriteria.AuthorQuery}")));
            }

            return pageableRequests;
        }

        private void AddBookPageableRequests(IndexerPageableRequestChain chain, SearchCriteriaBase searchCriteria, string parameters)
        {
            chain.AddTier();

            chain.Add(GetPagedRequests(MaxPages, Settings.Categories, "book", $"{parameters}"));
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, IEnumerable<int> categories, string searchType, string parameters)
        {
            if (categories.Empty())
            {
                yield break;
            }

            var categoriesQuery = string.Join(",", categories.Distinct());

            var baseUrl =
                $"{Settings.BaseUrl.TrimEnd('/')}{Settings.ApiPath.TrimEnd('/')}?t={searchType}&cat={categoriesQuery}&extended=1{Settings.AdditionalParameters}";

            if (Settings.ApiKey.IsNotNullOrWhiteSpace())
            {
                baseUrl += "&apikey=" + Settings.ApiKey;
            }

            if (PageSize == 0)
            {
                yield return new IndexerRequest($"{baseUrl}{parameters}", HttpAccept.Rss);
            }
            else
            {
                for (var page = 0; page < maxPages; page++)
                {
                    yield return new IndexerRequest($"{baseUrl}&offset={page * PageSize}&limit={PageSize}{parameters}",
                        HttpAccept.Rss);
                }
            }
        }

        private static string NewsnabifyTitle(string title)
        {
            return title.Replace("+", "%20");
        }
    }
}
