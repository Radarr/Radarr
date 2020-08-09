using System.Collections.Generic;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Trakt;

namespace NzbDrone.Core.NetImport.Trakt.List
{
    public class TraktListRequestGenerator : INetImportRequestGenerator
    {
        private readonly ITraktProxy _traktProxy;
        public TraktListSettings Settings { get; set; }

        public TraktListRequestGenerator(ITraktProxy traktProxy)
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

            var listName = Parser.Parser.ToUrlSlug(Settings.Listname.Trim());
            link += $"users/{Settings.Username.Trim()}/lists/{listName}/items/movies?limit={Settings.Limit}";

            var request = new NetImportRequest(_traktProxy.BuildTraktRequest(link, HttpMethod.GET, Settings.AccessToken));

            yield return request;
        }
    }
}
