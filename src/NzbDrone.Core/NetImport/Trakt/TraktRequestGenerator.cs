using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.NetImport.Trakt
{
    public class RefreshRequestResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
    }

    public class TraktRequestGenerator : INetImportRequestGenerator
    {
        public IConfigService _configService;
        public IHttpClient HttpClient { get; set; }
        public TraktSettings Settings { get; set; }

        public TraktRequestGenerator()
        {
        }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var pageableRequests = new NetImportPageableRequestChain();

            pageableRequests.Add(GetMovies(null));

            return pageableRequests;
        }

        private IEnumerable<NetImportRequest> GetMovies(string searchParameters)
        {
            var link = Settings.Link.Trim();

            var filtersAndLimit = $"?years={Settings.Years}&genres={Settings.Genres.ToLower()}&ratings={Settings.Rating}&certifications={Settings.Certification.ToLower()}&limit={Settings.Limit}{Settings.TraktAdditionalParameters}";

            switch (Settings.TraktListType)
            {
                case (int)TraktListType.UserCustomList:
                    var listName = Parser.Parser.ToUrlSlug(Settings.Listname.Trim());
                    link = link + $"/users/{Settings.Username.Trim()}/lists/{listName}/items/movies?limit={Settings.Limit}";
                    break;
                case (int)TraktListType.UserWatchList:
                    link = link + $"/users/{Settings.Username.Trim()}/watchlist/movies?limit={Settings.Limit}";
                    break;
                case (int)TraktListType.UserWatchedList:
                    link = link + $"/users/{Settings.Username.Trim()}/watched/movies?limit={Settings.Limit}";
                    break;
                case (int)TraktListType.Trending:
                    link = link + "/movies/trending" + filtersAndLimit;
                    break;
                case (int)TraktListType.Popular:
                    link = link + "/movies/popular" + filtersAndLimit;
                    break;
                case (int)TraktListType.Anticipated:
                    link = link + "/movies/anticipated" + filtersAndLimit;
                    break;
                case (int)TraktListType.BoxOffice:
                    link = link + "/movies/boxoffice" + filtersAndLimit;
                    break;
                case (int)TraktListType.TopWatchedByWeek:
                    link = link + "/movies/watched/weekly" + filtersAndLimit;
                    break;
                case (int)TraktListType.TopWatchedByMonth:
                    link = link + "/movies/watched/monthly" + filtersAndLimit;
                    break;
                case (int)TraktListType.TopWatchedByYear:
                    link = link + "/movies/watched/yearly" + filtersAndLimit;
                    break;
                case (int)TraktListType.TopWatchedByAllTime:
                    link = link + "/movies/watched/all" + filtersAndLimit;
                    break;
            }

            var request = new NetImportRequest($"{link}", HttpAccept.Json);

            request.HttpRequest.Headers.Add("trakt-api-version", "2");
            request.HttpRequest.Headers.Add("trakt-api-key", Settings.ClientId); //aeon

            if (Settings.AccessToken.IsNotNullOrWhiteSpace())
            {
                request.HttpRequest.Headers.Add("Authorization", "Bearer " + Settings.AccessToken);
            }

            yield return request;
        }
    }
}
