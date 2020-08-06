using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.RadarrList2
{
    public abstract class RadarrList2RequestGeneratorBase : IImportListRequestGenerator
    {
        public IHttpRequestBuilderFactory RequestBuilder { get; set; }
        public IHttpClient HttpClient { get; set; }
        public Logger Logger { get; set; }

        protected abstract HttpRequest GetHttpRequest();

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            var httpRequest = GetHttpRequest();

            var request = new ImportListRequest(httpRequest.Url.ToString(), HttpAccept.Json);

            request.HttpRequest.SuppressHttpError = true;

            pageableRequests.Add(new List<ImportListRequest> { request });
            return pageableRequests;
        }
    }
}
