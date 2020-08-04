using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.NetImport.Trakt.Popular
{
    public class TraktPopularRequestGenerator : INetImportRequestGenerator
    {
        public TraktPopularSettings Settings { get; set; }

        public string ClientId { get; set; }

        public TraktPopularRequestGenerator()
        {
        }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var pageableRequests = new NetImportPageableRequestChain();

            pageableRequests.Add(GetMoviesRequest());

            return pageableRequests;
        }

        private IEnumerable<NetImportRequest> GetMoviesRequest()
        {
            var link = Settings.Link.Trim();

            var filtersAndLimit = $"?years={Settings.Years}&genres={Settings.Genres.ToLower()}&ratings={Settings.Rating}&certifications={Settings.Certification.ToLower()}&limit={Settings.Limit}{Settings.TraktAdditionalParameters}";

            switch (Settings.TraktListType)
            {
                case (int)TraktPopularListType.Trending:
                    link += "/movies/trending" + filtersAndLimit;
                    break;
                case (int)TraktPopularListType.Popular:
                    link += "/movies/popular" + filtersAndLimit;
                    break;
                case (int)TraktPopularListType.Anticipated:
                    link += "/movies/anticipated" + filtersAndLimit;
                    break;
                case (int)TraktPopularListType.BoxOffice:
                    link += "/movies/boxoffice" + filtersAndLimit;
                    break;
                case (int)TraktPopularListType.TopWatchedByWeek:
                    link += "/movies/watched/weekly" + filtersAndLimit;
                    break;
                case (int)TraktPopularListType.TopWatchedByMonth:
                    link += "/movies/watched/monthly" + filtersAndLimit;
                    break;
                case (int)TraktPopularListType.TopWatchedByYear:
                    link += "/movies/watched/yearly" + filtersAndLimit;
                    break;
                case (int)TraktPopularListType.TopWatchedByAllTime:
                    link += "/movies/watched/all" + filtersAndLimit;
                    break;
            }

            var request = new NetImportRequest($"{link}", HttpAccept.Json);

            request.HttpRequest.Headers.Add("trakt-api-version", "2");
            request.HttpRequest.Headers.Add("trakt-api-key", ClientId); //aeon

            if (Settings.AccessToken.IsNotNullOrWhiteSpace())
            {
                request.HttpRequest.Headers.Add("Authorization", "Bearer " + Settings.AccessToken);
            }

            yield return request;
        }
    }
}
