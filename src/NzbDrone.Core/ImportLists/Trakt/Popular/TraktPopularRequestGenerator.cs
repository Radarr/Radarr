using System.Collections.Generic;
using System.Net.Http;
using NzbDrone.Core.Notifications.Trakt;

namespace NzbDrone.Core.ImportLists.Trakt.Popular
{
    public class TraktPopularRequestGenerator : IImportListRequestGenerator
    {
        private readonly ITraktProxy _traktProxy;
        public TraktPopularSettings Settings { get; set; }

        public TraktPopularRequestGenerator(ITraktProxy traktProxy)
        {
            _traktProxy = traktProxy;
        }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetMoviesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMoviesRequest()
        {
            var link = string.Empty;

            var filtersAndLimit = $"?years={Settings.Years}&genres={Settings.Genres.ToLower()}&ratings={Settings.Rating}&certifications={Settings.Certification.ToLower()}&limit={Settings.Limit}{Settings.TraktAdditionalParameters}";

            switch (Settings.TraktListType)
            {
                case (int)TraktPopularListType.Trending:
                    link += "movies/trending";
                    break;
                case (int)TraktPopularListType.Popular:
                    link += "movies/popular";
                    break;
                case (int)TraktPopularListType.Anticipated:
                    link += "movies/anticipated";
                    break;
                case (int)TraktPopularListType.BoxOffice:
                    link += "movies/boxoffice";
                    break;
                case (int)TraktPopularListType.TopWatchedByWeek:
                    link += "movies/watched/weekly";
                    break;
                case (int)TraktPopularListType.TopWatchedByMonth:
                    link += "movies/watched/monthly";
                    break;
                case (int)TraktPopularListType.TopWatchedByYear:
                    link += "movies/watched/yearly";
                    break;
                case (int)TraktPopularListType.TopWatchedByAllTime:
                    link += "movies/watched/all";
                    break;
                case (int)TraktPopularListType.RecommendedByWeek:
                    link += "movies/recommended/weekly";
                    break;
                case (int)TraktPopularListType.RecommendedByMonth:
                    link += "movies/recommended/monthly";
                    break;
                case (int)TraktPopularListType.RecommendedByYear:
                    link += "movies/recommended/yearly";
                    break;
                case (int)TraktPopularListType.RecommendedByAllTime:
                    link += "movies/recommended/yearly";
                    break;
            }

            link += filtersAndLimit;

            var request = new ImportListRequest(_traktProxy.BuildTraktRequest(link, HttpMethod.Get, Settings.AccessToken));

            yield return request;
        }
    }
}
