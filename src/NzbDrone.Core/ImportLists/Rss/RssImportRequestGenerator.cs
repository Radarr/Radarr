using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Rss
{
    public class RssImportRequestGenerator : IImportListRequestGenerator
    {
        public RssImportBaseSettings Settings { get; set; }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetMoviesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMoviesRequest()
        {
            var request = new ImportListRequest(Settings.Url, HttpAccept.Rss);

            yield return request;
        }
    }
}
