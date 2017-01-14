using System;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.TorrentPotato
{
    public class TorrentPotatoRequestGenerator : IIndexerRequestGenerator
    {

        public TorrentPotatoSettings Settings { get; set; }

        public TorrentPotatoRequestGenerator()
        {

        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests("list", null, null));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests("search", searchCriteria.Series.TvdbId, "S{0:00}E{1:00}", searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests("search", searchCriteria.Series.TvdbId, "S{0:00}", searchCriteria.SeasonNumber));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests("search", searchCriteria.Series.TvdbId, "\"{0:yyyy MM dd}\"", searchCriteria.AirDate));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            foreach (var queryTitle in searchCriteria.EpisodeQueryTitles)
            {
                var query = queryTitle.Replace('+', ' ');
                query = System.Web.HttpUtility.UrlEncode(query);

                pageableRequests.Add(GetPagedRequests("search", searchCriteria.Series.TvdbId, query));
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(string mode, int? tvdbId, string query, params object[] args)
        {
            var requestBuilder = new HttpRequestBuilder(Settings.BaseUrl)
                .Accept(HttpAccept.Json);

            requestBuilder.AddQueryParam("passkey", Settings.Passkey);
            if (!string.IsNullOrWhiteSpace(Settings.User))
            {
                requestBuilder.AddQueryParam("user", Settings.User);
            }
            else
            {
                requestBuilder.AddQueryParam("user", "");
            }

            requestBuilder.AddQueryParam("search", "the");

            yield return new IndexerRequest(requestBuilder.Build());
        }

        private IEnumerable<IndexerRequest> GetMovieRequest(MovieSearchCriteria searchCriteria)
        {
            var requestBuilder = new HttpRequestBuilder(Settings.BaseUrl)
                 .Accept(HttpAccept.Json);

            requestBuilder.AddQueryParam("passkey", Settings.Passkey);

            if (!string.IsNullOrWhiteSpace(Settings.User))
            {
                requestBuilder.AddQueryParam("user", Settings.User);
            }
            else
            {
                requestBuilder.AddQueryParam("user", "");
            }

            if (searchCriteria.Movie.ImdbId.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("imdbid", searchCriteria.Movie.ImdbId);
            }
            else
            {
                requestBuilder.AddQueryParam("search", searchCriteria.Movie.Title);
            }

            yield return new IndexerRequest(requestBuilder.Build());
        }

        public IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            

            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetMovieRequest(searchCriteria));

            return pageableRequests;

            
        }
    }
}
