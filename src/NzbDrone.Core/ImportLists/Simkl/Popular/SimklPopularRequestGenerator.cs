using System.Collections.Generic;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Simkl;

namespace NzbDrone.Core.ImportLists.Simkl.Popular
{
    public class SimklPopularRequestGenerator : IImportListRequestGenerator
    {
        private readonly ISimklProxy _simklProxy;
        public SimklPopularSettings Settings { get; set; }

        public SimklPopularRequestGenerator(ISimklProxy simklProxy)
        {
            _simklProxy = simklProxy;
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

            var filtersAndLimit = $"?years={Settings.Years}&genres={Settings.Genres.ToLower()}&ratings={Settings.Rating}&certifications={Settings.Certification.ToLower()}&limit={Settings.Limit}{Settings.SimklAdditionalParameters}";

            switch (Settings.SimklListType)
            {
                case (int)SimklPopularListType.Trending:
                    link += "movies/trending" + filtersAndLimit;
                    break;
                case (int)SimklPopularListType.Popular:
                    link += "movies/popular" + filtersAndLimit;
                    break;
                case (int)SimklPopularListType.Anticipated:
                    link += "movies/anticipated" + filtersAndLimit;
                    break;
                case (int)SimklPopularListType.BoxOffice:
                    link += "movies/boxoffice" + filtersAndLimit;
                    break;
                case (int)SimklPopularListType.TopWatchedByWeek:
                    link += "movies/watched/weekly" + filtersAndLimit;
                    break;
                case (int)SimklPopularListType.TopWatchedByMonth:
                    link += "movies/watched/monthly" + filtersAndLimit;
                    break;
                case (int)SimklPopularListType.TopWatchedByYear:
                    link += "movies/watched/yearly" + filtersAndLimit;
                    break;
                case (int)SimklPopularListType.TopWatchedByAllTime:
                    link += "movies/watched/all" + filtersAndLimit;
                    break;
            }

            var request = new ImportListRequest(_simklProxy.BuildSimklRequest(link, HttpMethod.GET, Settings.AccessToken));

            yield return request;
        }
    }
}
