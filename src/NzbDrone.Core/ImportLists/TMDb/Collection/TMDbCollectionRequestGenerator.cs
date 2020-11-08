using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.TMDb.Collection
{
    public class TMDbCollectionRequestGenerator : IImportListRequestGenerator
    {
        public TMDbCollectionSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public IHttpRequestBuilderFactory RequestBuilder { get; set; }
        public Logger Logger { get; set; }

        public TMDbCollectionRequestGenerator()
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
            Logger.Info($"Importing TMDb movies from collection: {Settings.CollectionId}");

            yield return new ImportListRequest(RequestBuilder.Create()
                                                            .SetSegment("api", "3")
                                                            .SetSegment("route", "collection")
                                                            .SetSegment("id", Settings.CollectionId)
                                                            .SetSegment("secondaryRoute", "")
                                                            .Build());
        }
    }
}
