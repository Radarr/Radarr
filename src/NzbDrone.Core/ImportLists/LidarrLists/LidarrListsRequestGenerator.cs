using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.LidarrLists
{
    public class LidarrListsRequestGenerator : IImportListRequestGenerator
    {
        public LidarrListsSettings Settings { get; set; }

        public int MaxPages { get; set; }
        public int PageSize { get; set; }

        public LidarrListsRequestGenerator()
        {
            MaxPages = 1;
            PageSize = 10;
        }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetPagedRequests());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetPagedRequests()
        {
            yield return new ImportListRequest(string.Format("{0}{1}", Settings.BaseUrl, Settings.ListId), HttpAccept.Json);
        }

    }
}
