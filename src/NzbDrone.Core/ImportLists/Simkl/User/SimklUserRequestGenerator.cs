using System.Collections.Generic;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Simkl;

namespace NzbDrone.Core.ImportLists.Simkl.User
{
    public class SimklUserRequestGenerator : IImportListRequestGenerator
    {
        private readonly ISimklProxy _simklProxy;
        public SimklUserSettings Settings { get; set; }

        public SimklUserRequestGenerator(ISimklProxy simklProxy)
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

            switch (Settings.SimklListType)
            {
                case (int)SimklUserListType.UserWatchList:
                    link += $"users/{Settings.AuthUser.Trim()}/watchlist/movies?limit={Settings.Limit}";
                    break;
                case (int)SimklUserListType.UserWatchedList:
                    link += $"users/{Settings.AuthUser.Trim()}/watched/movies?limit={Settings.Limit}";
                    break;
                case (int)SimklUserListType.UserCollectionList:
                    link += $"users/{Settings.AuthUser.Trim()}/collection/movies?limit={Settings.Limit}";
                    break;
            }

            var request = new ImportListRequest(_simklProxy.BuildSimklRequest(link, HttpMethod.GET, Settings.AccessToken));

            yield return request;
        }
    }
}
