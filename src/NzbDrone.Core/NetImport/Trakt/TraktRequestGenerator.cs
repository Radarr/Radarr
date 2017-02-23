using NzbDrone.Common.Http;
using System.Collections.Generic;

namespace NzbDrone.Core.NetImport.Trakt
{
    public class TraktRequestGenerator : INetImportRequestGenerator
    {
        public TraktSettings Settings { get; set; }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var pageableRequests = new NetImportPageableRequestChain();

            pageableRequests.Add(GetMovies(null));

            return pageableRequests;
        }

        private IEnumerable<NetImportRequest> GetMovies(string searchParameters)
        {
            var link = Settings.Link.Trim();

            var filters = $"?years={Settings.Years}&genres={Settings.Genres.ToLower()}&ratings={Settings.Rating}&certifications={Settings.Ceritification.ToLower()}";

            switch (Settings.ListType)
            {
                case (int)TraktListType.UserCustomList:
                    link = link + $"/users/{Settings.Username.Trim()}/lists/{Settings.Listname.Trim()}/items/movies";
                    break;
                case (int)TraktListType.UserWatchList:
                    link = link + $"/users/{Settings.Username.Trim()}/watchlist/movies";
                    break;
                case (int)TraktListType.UserWatchedList:
                    link = link + $"/users/{Settings.Username.Trim()}/watched/movies";
                    break;
                case (int)TraktListType.Trending:
                    link = link + "/movies/trending" + filters;
                    break;
                case (int)TraktListType.Popular:
                    link = link + "/movies/popular" + filters;
                    break;
                case (int)TraktListType.Anticipated:
                    link = link + "/movies/anticipated" + filters;
                    break;
                case (int)TraktListType.BoxOffice:
                    link = link + "/movies/boxoffice" + filters;
                    break;
                case (int)TraktListType.TopWatchedByWeek:
                    link = link + "/movies/watched/weekly" + filters;
                    break;
                case (int)TraktListType.TopWatchedByMonth:
                    link = link + "/movies/watched/monthly" + filters;
                    break;
                case (int)TraktListType.TopWatchedByYear:
                    link = link + "/movies/watched/yearly" + filters;
                    break;
                case (int)TraktListType.TopWatchedByAllTime:
                    link = link + "/movies/watched/all" + filters;
                    break;
            }

            var request = new NetImportRequest($"{link}", HttpAccept.Json);
            request.HttpRequest.Headers.Add("trakt-api-version", "2");
            request.HttpRequest.Headers.Add("trakt-api-key", "657bb899dcb81ec8ee838ff09f6e013ff7c740bf0ccfa54dd41e791b9a70b2f0");

            yield return request;
        }
    }
}
