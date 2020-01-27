using System;
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

        private bool SupportsMovieSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedMovieSearchParameters != null &&
                       capabilities.SupportedMovieSearchParameters.Contains("imdbid");
            }
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

            // Some indexers might forget to enable movie search, but normal search still works fine. Thus we force a normal search.
            if (capabilities.SupportedMovieSearchParameters != null)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories.Concat(Settings.AnimeCategories), "movie", ""));
            }
            else if (capabilities.SupportedSearchParameters != null)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories.Concat(Settings.AnimeCategories), "search", ""));
            }

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (!Settings.SearchByTitle && SupportsMovieSearch && searchCriteria.Movie.ImdbId.IsNotNullOrWhiteSpace())
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories, "movie", $"&imdbid={searchCriteria.Movie.ImdbId.Substring(2)}"));
            }
            else
            {
                var altTitles = searchCriteria.Movie.AlternativeTitles.Take(5).Select(t => t.Title).ToList();
                altTitles.Add(searchCriteria.Movie.Title);

                var realMaxPages = (int)MaxPages / altTitles.Count();

                //pageableRequests.Add(GetPagedRequests(MaxPages - (altTitles.Count() * realMaxPages), Settings.Categories, "search", $"&q={searchTitle}%20{searchCriteria.Movie.Year}"));

                //Also use alt titles for searching.
                foreach (string altTitle in altTitles)
                {
                    var searchAltTitle = System.Web.HttpUtility.UrlPathEncode(Parser.Parser.ReplaceGermanUmlauts(Parser.Parser.NormalizeTitle(altTitle)));
                    var queryString = $"&q={searchAltTitle}";
                    if (!Settings.RemoveYear)
                    {
                        queryString += $"%20{searchCriteria.Movie.Year}";
                    }

                    pageableRequests.Add(GetPagedRequests(realMaxPages, Settings.Categories, "search", queryString));
                }
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, IEnumerable<int> categories, string searchType, string parameters)
        {
            if (categories.Empty())
            {
                yield break;
            }

            var categoriesQuery = string.Join(",", categories.Distinct());

            var baseUrl = string.Format("{0}{1}?t={2}&cat={3}&extended=1{4}", Settings.BaseUrl.TrimEnd('/'), Settings.ApiPath.TrimEnd('/'), searchType, categoriesQuery, Settings.AdditionalParameters);

            if (Settings.ApiKey.IsNotNullOrWhiteSpace())
            {
                baseUrl += "&apikey=" + Settings.ApiKey;
            }

            if (PageSize == 0)
            {
                yield return new IndexerRequest(string.Format("{0}{1}", baseUrl, parameters), HttpAccept.Rss);
            }
            else
            {
                for (var page = 0; page < maxPages; page++)
                {
                    yield return new IndexerRequest(string.Format("{0}&offset={1}&limit={2}{3}", baseUrl, page * PageSize, PageSize, parameters), HttpAccept.Rss);
                }
            }
        }

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }
}
