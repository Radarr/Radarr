using System.Collections.Generic;
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

        public virtual IndexerPageableRequestChain GetSearchRequests(AlbumSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests("search", null, "{0}+{1}", searchCriteria.ArtistQuery, searchCriteria.AlbumQuery));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(ArtistSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests("search", null, "{0}", searchCriteria.ArtistQuery));

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(string mode, int? tvdbId, string query, params object[] args)
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

            if (tvdbId.HasValue)
            {
                requestBuilder.AddQueryParam("search_tvdb", tvdbId.Value);
            }

            if (query.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("search_string", string.Format(query, args));
            }

            if (!Settings.RankedOnly)
            {
                requestBuilder.AddQueryParam("ranked", "0");
            }

            requestBuilder.AddQueryParam("category", "1;23;24;25;26");
            requestBuilder.AddQueryParam("limit", "100");
            requestBuilder.AddQueryParam("token", _tokenProvider.GetToken(Settings));
            requestBuilder.AddQueryParam("format", "json_extended");
            requestBuilder.AddQueryParam("app_id", BuildInfo.AppName);

            yield return new IndexerRequest(requestBuilder.Build());
        }
    }
}
