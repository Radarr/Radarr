using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.NetImport.RadarrList2
{
    public abstract class RadarrList2RequestGeneratorBase : INetImportRequestGenerator
    {
        public IHttpRequestBuilderFactory RequestBuilder { get; set; }
        public IHttpClient HttpClient { get; set; }
        public Logger Logger { get; set; }

        protected abstract HttpRequest GetHttpRequest();

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var pageableRequests = new NetImportPageableRequestChain();

            var httpRequest = GetHttpRequest();

            var request = new NetImportRequest(httpRequest.Url.ToString(), HttpAccept.Json);

            request.HttpRequest.SuppressHttpError = true;

            pageableRequests.Add(new List<NetImportRequest> { request });
            return pageableRequests;
        }
    }
}
