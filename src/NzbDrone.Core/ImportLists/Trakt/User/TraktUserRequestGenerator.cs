using System.Collections.Generic;
using System.Net.Http;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Trakt;

namespace NzbDrone.Core.ImportLists.Trakt.User
{
    public class TraktUserRequestGenerator : IImportListRequestGenerator
    {
        private readonly ITraktProxy _traktProxy;
        public TraktUserSettings Settings { get; set; }

        public TraktUserRequestGenerator(ITraktProxy traktProxy)
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
            var userName = Settings.Username.IsNotNullOrWhiteSpace() ? Settings.Username.Trim() : Settings.AuthUser.Trim();

            switch (Settings.TraktListType)
            {
                case (int)TraktUserListType.UserWatchList:
                    link += $"users/{userName}/watchlist/movies?limit={Settings.Limit}";
                    break;
                case (int)TraktUserListType.UserWatchedList:
                    link += $"users/{userName}/watched/movies?limit={Settings.Limit}";
                    break;
                case (int)TraktUserListType.UserCollectionList:
                    link += $"users/{userName}/collection/movies?limit={Settings.Limit}";
                    break;
            }

            var request = new ImportListRequest(_traktProxy.BuildTraktRequest(link, HttpMethod.Get, Settings.AccessToken));

            yield return request;
        }
    }
}
