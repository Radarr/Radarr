using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.MyAnonamouse
{
    public class MyAnonamouseRequestGenerator : IIndexerRequestGenerator
    {
        private static readonly Regex SanitizeSearchQueryRegex = new ("[^\\w]+", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1));

        public MyAnonamouseSettings Settings { get; set; }

        private readonly Logger _logger;

        public MyAnonamouseRequestGenerator(MyAnonamouseSettings settings, Logger logger)
        {
            Settings = settings;
            _logger = logger;
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();
            pageableRequests.Add(GetPagedRequests(string.Empty));
            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public IndexerPageableRequestChain GetSearchRequests(BookSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            var searchTerm = BuildSearchTerm(searchCriteria);

            if (searchTerm.IsNotNullOrWhiteSpace())
            {
                pageableRequests.Add(GetPagedRequests(searchTerm));
            }

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(AudiobookSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            var searchTerm = BuildSearchTerm(searchCriteria);

            if (searchTerm.IsNotNullOrWhiteSpace())
            {
                pageableRequests.Add(GetPagedRequests(searchTerm));
            }

            return pageableRequests;
        }

        private static string BuildSearchTerm(SearchCriteriaBase searchCriteria)
        {
            var terms = new List<string>();

            if (searchCriteria is BookSearchCriteria bookCriteria)
            {
                if (bookCriteria.Author.IsNotNullOrWhiteSpace())
                {
                    terms.Add(bookCriteria.Author);
                }

                if (bookCriteria.Title.IsNotNullOrWhiteSpace())
                {
                    terms.Add(bookCriteria.Title);
                }
            }
            else if (searchCriteria is AudiobookSearchCriteria audiobookCriteria)
            {
                if (audiobookCriteria.Author.IsNotNullOrWhiteSpace())
                {
                    terms.Add(audiobookCriteria.Author);
                }

                if (audiobookCriteria.Title.IsNotNullOrWhiteSpace())
                {
                    terms.Add(audiobookCriteria.Title);
                }
            }

            return string.Join(" ", terms);
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(string term)
        {
            var sanitizedTerm = SanitizeSearchQueryRegex.Replace(term, " ").Trim();

            if (term.IsNotNullOrWhiteSpace() && sanitizedTerm.IsNullOrWhiteSpace())
            {
                _logger.Debug("Search term is empty after sanitization, skipping. Original: '{0}'", term);
                yield break;
            }

            var searchType = Settings.SearchType switch
            {
                (int)MyAnonamouseSearchType.Active => "active",
                (int)MyAnonamouseSearchType.Freeleech => "fl",
                (int)MyAnonamouseSearchType.FreeleechOrVip => "fl-VIP",
                (int)MyAnonamouseSearchType.Vip => "VIP",
                (int)MyAnonamouseSearchType.NotVip => "nVIP",
                _ => "all"
            };

            var parameters = new NameValueCollection
            {
                { "tor[text]", sanitizedTerm },
                { "tor[searchType]", searchType },
                { "tor[srchIn][title]", "true" },
                { "tor[srchIn][author]", "true" },
                { "tor[srchIn][narrator]", "true" },
                { "tor[searchIn]", "torrents" },
                { "tor[sortType]", "default" },
                { "tor[perpage]", "100" },
                { "tor[startNumber]", "0" },
                { "thumbnails", "1" },
                { "description", "1" }
            };

            if (Settings.SearchInDescription)
            {
                parameters.Set("tor[srchIn][description]", "true");
            }

            if (Settings.SearchInSeries)
            {
                parameters.Set("tor[srchIn][series]", "true");
            }

            if (Settings.SearchInFilenames)
            {
                parameters.Set("tor[srchIn][filenames]", "true");
            }

            parameters.Set("tor[cat][]", "0");

            var searchUrl = Settings.BaseUrl.TrimEnd('/') + "/tor/js/loadSearchJSONbasic.php";

            if (parameters.Count > 0)
            {
                searchUrl += "?" + parameters.ToQueryString();
            }

            var requestBuilder = new HttpRequestBuilder(searchUrl)
                .Accept(HttpAccept.Json);

            var cookies = GetCookies?.Invoke();

            if (cookies != null && cookies.TryGetValue("mam_id", out var mamId) && mamId.IsNotNullOrWhiteSpace())
            {
                requestBuilder.SetCookies(cookies);
            }
            else if (Settings.MamId.IsNotNullOrWhiteSpace())
            {
                requestBuilder.SetCookies(new Dictionary<string, string> { { "mam_id", Settings.MamId } });
            }

            yield return new IndexerRequest(requestBuilder.Build());
        }

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }

    internal static class NameValueCollectionExtensions
    {
        public static string ToQueryString(this NameValueCollection nvc)
        {
            var items = new List<string>();

            foreach (var key in nvc.AllKeys)
            {
                items.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(nvc[key])}");
            }

            return string.Join("&", items);
        }
    }
}
