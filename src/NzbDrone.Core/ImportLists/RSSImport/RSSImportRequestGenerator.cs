using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.RSSImport
{
    public class RSSImportRequestGenerator : IImportListRequestGenerator
    {
        public RSSImportSettings Settings { get; set; }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetMovies(null));

            return pageableRequests;
        }

        //public ImportListPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        //{
        //    return new ImportListPageableRequestChain();
        //}
        private IEnumerable<ImportListRequest> GetMovies(string searchParameters)
        {
            var request = new ImportListRequest($"{Settings.Link.Trim()}", HttpAccept.Rss);
            yield return request;
        }
    }
}
