using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.NetImport.TMDb.List
{
    public class TMDbListRequestGenerator : INetImportRequestGenerator
    {
        public TMDbListSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public IHttpRequestBuilderFactory RequestBuilder { get; set; }
        public Logger Logger { get; set; }

        public TMDbListRequestGenerator()
        {
        }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var pageableRequests = new NetImportPageableRequestChain();

            pageableRequests.Add(GetMoviesRequest());

            return pageableRequests;
        }

        private IEnumerable<NetImportRequest> GetMoviesRequest()
        {
            Logger.Info($"Importing TMDb movies from list: {Settings.ListId}");

            var requestBuilder = RequestBuilder.Create()
                                               .SetSegment("api", "3")
                                               .SetSegment("route", "list")
                                               .SetSegment("id", Settings.ListId)
                                               .SetSegment("secondaryRoute", "");

            yield return new NetImportRequest(requestBuilder.Accept(HttpAccept.Json)
                                                            .Build());
        }
    }
}
