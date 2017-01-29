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
            var link = $"{Settings.Link.Trim()}{Settings.Username.Trim()}";

            switch (Settings.ListType)
            {
                case (int)TraktListType.CustomList:
                    link = link + $"/lists/{Settings.Listname.Trim()}/items/movies";
                    break;
                case (int)TraktListType.WatchList:
                    link = link + "/watchlist/movies";
                    break;
                case (int)TraktListType.Watched:
                    link = link + "/watched/movies";
                    break;
            }

            var request = new NetImportRequest($"{link}", HttpAccept.Json);
            request.HttpRequest.Headers.Add("trakt-api-version", "2");
            request.HttpRequest.Headers.Add("trakt-api-key", "657bb899dcb81ec8ee838ff09f6e013ff7c740bf0ccfa54dd41e791b9a70b2f0");

            yield return request;
        }
    }
}
