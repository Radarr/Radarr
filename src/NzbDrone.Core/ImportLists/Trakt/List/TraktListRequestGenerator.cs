using System.Collections.Generic;
using System.Net.Http;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Trakt;

namespace NzbDrone.Core.ImportLists.Trakt.List
{
    public class TraktListRequestGenerator : IImportListRequestGenerator
    {
        private readonly ITraktProxy _traktProxy;
        public TraktListSettings Settings { get; set; }

        public TraktListRequestGenerator(ITraktProxy traktProxy)
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

            var listName = Parser.Parser.ToUrlSlug(Settings.Listname.Trim());
            link += $"users/{Settings.Username.Trim()}/lists/{listName}/items/movies?limit={Settings.Limit}";

            var request = new ImportListRequest(_traktProxy.BuildTraktRequest(link, HttpMethod.Get, Settings.AccessToken));

            yield return request;
        }
    }
}
