using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.TMDb.List
{
    public class TMDbListRequestGenerator : IImportListRequestGenerator
    {
        public TMDbListSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public IHttpRequestBuilderFactory RequestBuilder { get; set; }
        public Logger Logger { get; set; }

        public TMDbListRequestGenerator()
        {
        }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetMoviesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMoviesRequest()
        {
            Logger.Info($"Importing TMDb movies from list: {Settings.ListId}");

            var requestBuilder = RequestBuilder.Create()
                                               .SetSegment("api", "3")
                                               .SetSegment("route", "list")
                                               .SetSegment("id", Settings.ListId)
                                               .SetSegment("secondaryRoute", "");

            yield return new ImportListRequest(requestBuilder.Accept(HttpAccept.Json)
                                                            .Build());
        }
    }
}
