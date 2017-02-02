using NzbDrone.Common.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                case (int)TraktListType.TrendingMovies:
                    link = link + "/movies/trending";
                    break;
                case (int)TraktListType.PopularMovies:
                    link = link + "/movies/popular";
                    break;
                case (int)TraktListType.AnticipatedMovies:
                    link = link + "/movies/anticipated";
                    break;
                case (int)TraktListType.BoxOfficeMovies:
                    link = link + "/movies/boxoffice";
                    break;
                case (int)TraktListType.TopWatchedByWeek:
                    link = link + "/movies/watched/weekly";
                    break;
                case (int)TraktListType.TopWatchedByMonth:
                    link = link + "/movies/watched/monthly";
                    break;
                case (int)TraktListType.TopWatchedByYear:
                    link = link + "/movies/watched/yearly";
                    break;
                case (int)TraktListType.TopWatchedByAllTime:
                    link = link + "/movies/watched/all";
                    break;
            }

            var request = new NetImportRequest($"{link}", HttpAccept.Json);
            request.HttpRequest.Headers.Add("trakt-api-version", "2");
            request.HttpRequest.Headers.Add("trakt-api-key", "657bb899dcb81ec8ee838ff09f6e013ff7c740bf0ccfa54dd41e791b9a70b2f0");

            yield return request;
        }
    }
}
