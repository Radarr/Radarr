using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.NetImport.Trakt.User
{
    public class TraktUserRequestGenerator : INetImportRequestGenerator
    {
        public TraktUserSettings Settings { get; set; }

        public string ClientId { get; set; }

        public TraktUserRequestGenerator()
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

            switch (Settings.TraktListType)
            {
                case (int)TraktUserListType.UserWatchList:
                    link += $"/users/{Settings.AuthUser.Trim()}/watchlist/movies?limit={Settings.Limit}";
                    break;
                case (int)TraktUserListType.UserWatchedList:
                    link += $"/users/{Settings.AuthUser.Trim()}/watched/movies?limit={Settings.Limit}";
                    break;
                case (int)TraktUserListType.UserCollectionList:
                    link += $"/users/{Settings.AuthUser.Trim()}/collection/movies?limit={Settings.Limit}";
                    break;
            }

            var request = new NetImportRequest($"{link}", HttpAccept.Json);

            request.HttpRequest.Headers.Add("trakt-api-version", "2");
            request.HttpRequest.Headers.Add("trakt-api-key", ClientId);

            if (Settings.AccessToken.IsNotNullOrWhiteSpace())
            {
                request.HttpRequest.Headers.Add("Authorization", "Bearer " + Settings.AccessToken);
            }

            yield return request;
        }
    }
}
