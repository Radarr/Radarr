using System.Collections.Generic;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Trakt;

namespace NzbDrone.Core.NetImport.Trakt.User
{
    public class TraktUserRequestGenerator : INetImportRequestGenerator
    {
        private readonly ITraktProxy _traktProxy;
        public TraktUserSettings Settings { get; set; }

        public TraktUserRequestGenerator(ITraktProxy traktProxy)
        {
            _traktProxy = traktProxy;
        }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var pageableRequests = new NetImportPageableRequestChain();

            pageableRequests.Add(GetMoviesRequest());

            return pageableRequests;
        }

        private IEnumerable<NetImportRequest> GetMoviesRequest()
        {
            var link = string.Empty;

            switch (Settings.TraktListType)
            {
                case (int)TraktUserListType.UserWatchList:
                    link += $"users/{Settings.AuthUser.Trim()}/watchlist/movies?limit={Settings.Limit}";
                    break;
                case (int)TraktUserListType.UserWatchedList:
                    link += $"users/{Settings.AuthUser.Trim()}/watched/movies?limit={Settings.Limit}";
                    break;
                case (int)TraktUserListType.UserCollectionList:
                    link += $"users/{Settings.AuthUser.Trim()}/collection/movies?limit={Settings.Limit}";
                    break;
            }

            var request = new NetImportRequest(_traktProxy.BuildTraktRequest(link, HttpMethod.GET, Settings.AccessToken));

            yield return request;
        }
    }
}
