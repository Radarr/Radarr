using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Rss
{
    public class RssImportRequestGenerator<TSettings> : IImportListRequestGenerator
        where TSettings : RssImportBaseSettings<TSettings>, new()
    {
        public RssImportBaseSettings<TSettings> Settings { get; set; }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetMoviesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMoviesRequest()
        {
            yield return new ImportListRequest(Settings.Url, HttpAccept.Rss);
        }
    }
}
