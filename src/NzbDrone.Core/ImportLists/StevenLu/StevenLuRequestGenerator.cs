using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.StevenLu
{
    public class StevenLuRequestGenerator : IImportListRequestGenerator
    {
        public StevenLuSettings Settings { get; set; }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();
            pageableRequests.Add(GetMovies(null));
            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMovies(string searchParameters)
        {
            var request = new ImportListRequest($"{Settings.Link.Trim()}", HttpAccept.Json);
            yield return request;
        }
    }
}
