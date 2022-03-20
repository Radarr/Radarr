using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Rarbg
{
    public class RarbgRequestGenerator : IIndexerRequestGenerator
    {
        private readonly IRarbgTokenProvider _tokenProvider;

        public RarbgSettings Settings { get; set; }

        public RarbgRequestGenerator(IRarbgTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();
            pageableRequests.Add(GetPagedRequests("list", null, null));
            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();
            pageableRequests.Add(GetMovieRequest(searchCriteria));
            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(string mode, int? imdbId, string query, params object[] args)
        {
            var requestBuilder = new HttpRequestBuilder(Settings.BaseUrl)
                .Resource("/pubapi_v2.php")
                .Accept(HttpAccept.Json);

            if (Settings.CaptchaToken.IsNotNullOrWhiteSpace())
            {
                requestBuilder.UseSimplifiedUserAgent = true;
                requestBuilder.SetCookie("cf_clearance", Settings.CaptchaToken);
            }

            requestBuilder.AddQueryParam("mode", mode);

            if (imdbId.HasValue)
            {
                requestBuilder.AddQueryParam("search_imdb", imdbId.Value);
            }

            if (query.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("search_string", string.Format(query, args));
            }

            if (!Settings.RankedOnly)
            {
                requestBuilder.AddQueryParam("ranked", "0");
            }

            var categoryParam = string.Join(";", Settings.Categories.Distinct());

            requestBuilder.AddQueryParam("category", categoryParam);
            requestBuilder.AddQueryParam("limit", "100");
            requestBuilder.AddQueryParam("token", _tokenProvider.GetToken(Settings));
            requestBuilder.AddQueryParam("format", "json_extended");
            requestBuilder.AddQueryParam("app_id", "Radarr");

            yield return new IndexerRequest(requestBuilder.Build());
        }

        private IEnumerable<IndexerRequest> GetMovieRequest(MovieSearchCriteria searchCriteria)
        {
            var requestBuilder = new HttpRequestBuilder(Settings.BaseUrl)
                .Resource("/pubapi_v2.php")
                .Accept(HttpAccept.Json);

            if (Settings.CaptchaToken.IsNotNullOrWhiteSpace())
            {
                requestBuilder.UseSimplifiedUserAgent = true;
                requestBuilder.SetCookie("cf_clearance", Settings.CaptchaToken);
            }

            requestBuilder.AddQueryParam("mode", "search");

            if (searchCriteria.Movie.MovieMetadata.Value.ImdbId.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("search_imdb", searchCriteria.Movie.MovieMetadata.Value.ImdbId);
            }
            else if (searchCriteria.Movie.MovieMetadata.Value.TmdbId > 0)
            {
                requestBuilder.AddQueryParam("search_themoviedb", searchCriteria.Movie.TmdbId);
            }
            else
            {
                requestBuilder.AddQueryParam("search_string", $"{searchCriteria.Movie.Title} {searchCriteria.Movie.Year}");
            }

            if (!Settings.RankedOnly)
            {
                requestBuilder.AddQueryParam("ranked", "0");
            }

            var categoryParam = string.Join(";", Settings.Categories.Distinct());

            requestBuilder.AddQueryParam("category", categoryParam);
            requestBuilder.AddQueryParam("limit", "100");
            requestBuilder.AddQueryParam("token", _tokenProvider.GetToken(Settings));
            requestBuilder.AddQueryParam("format", "json_extended");
            requestBuilder.AddQueryParam("app_id", BuildInfo.AppName);

            yield return new IndexerRequest(requestBuilder.Build());
        }

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }
}
