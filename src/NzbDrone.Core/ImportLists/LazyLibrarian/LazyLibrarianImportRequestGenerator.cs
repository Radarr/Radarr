using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.LazyLibrarianImport
{
    public class LazyLibrarianImportRequestGenerator : IImportListRequestGenerator
    {
        public LazyLibrarianImportSettings Settings { get; set; }

        public int MaxPages { get; set; }
        public int PageSize { get; set; }

        public LazyLibrarianImportRequestGenerator()
        {
            MaxPages = 1;
            PageSize = 1000;
        }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetPagedRequests());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetPagedRequests()
        {
            yield return new ImportListRequest(string.Format("{0}/api?cmd=getAllBooks&apikey={1}", Settings.BaseUrl.TrimEnd('/'), Settings.ApiKey), HttpAccept.Json);
        }
    }
}
