using System.Collections.Generic;
using System.Net.Http;
using NzbDrone.Core.Notifications.Trakt;

namespace NzbDrone.Core.ImportLists.Trakt.SmartList
{
    public class TraktSmartListRequestGenerator : IImportListRequestGenerator
    {
        private readonly ITraktProxy _traktProxy;
        public TraktSmartListSettings Settings { get; set; }

        public TraktSmartListRequestGenerator(ITraktProxy traktProxy)
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
            var link = $"smart-lists/{TraktSmartListSlug.Parse(Settings.SmartList)}/items/movies?limit={Settings.Limit}";

            yield return new ImportListRequest(_traktProxy.BuildRequest(link, HttpMethod.Get, Settings.AccessToken));
        }
    }
}
