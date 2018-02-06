using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.HeadphonesImport
{
    public class HeadphonesImportRequestGenerator : IImportListRequestGenerator
    {
        public HeadphonesImportSettings Settings { get; set; }

        public int MaxPages { get; set; }
        public int PageSize { get; set; }

        public HeadphonesImportRequestGenerator()
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
            yield return new ImportListRequest(string.Format("{0}/api?cmd=getIndex&apikey={1}", Settings.BaseUrl.TrimEnd('/'), Settings.ApiKey), HttpAccept.Json);
        }

    }
}
