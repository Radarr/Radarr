using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.RadarrList
{
    public class RadarrListRequestGenerator : IImportListRequestGenerator
    {
        public RadarrListSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public Logger Logger { get; set; }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            var request = new ImportListRequest(Settings.Url, HttpAccept.Json);

            request.HttpRequest.SuppressHttpError = true;

            pageableRequests.Add(new List<ImportListRequest> { request });
            return pageableRequests;
        }
    }
}
