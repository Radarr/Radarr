using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.TMDb.Person
{
    public class TMDbPersonRequestGenerator : IImportListRequestGenerator
    {
        public TMDbPersonSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public IHttpRequestBuilderFactory RequestBuilder { get; set; }
        public Logger Logger { get; set; }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetMoviesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMoviesRequest()
        {
            Logger.Info($"Importing TMDb movies from person: {Settings.PersonId}");

            var requestBuilder = RequestBuilder.Create()
                                               .SetSegment("api", "3")
                                               .SetSegment("route", "person")
                                               .SetSegment("id", Settings.PersonId)
                                               .SetSegment("secondaryRoute", "/movie_credits");

            yield return new ImportListRequest(requestBuilder.Accept(HttpAccept.Json)
                                                            .Build());
        }
    }
}
