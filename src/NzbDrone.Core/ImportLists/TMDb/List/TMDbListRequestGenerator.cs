using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.TMDb.List
{
    public class TMDbListRequestGenerator : IImportListRequestGenerator
    {
        public TMDbListSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public IHttpRequestBuilderFactory RequestBuilder { get; set; }
        public Logger Logger { get; set; }
        public int MaxPages { get; set; }

        public TMDbListRequestGenerator()
        {
            MaxPages = 5;
        }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetMoviesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMoviesRequest()
        {
            for (var pageNumber = 1; pageNumber <= MaxPages; pageNumber++)
            {
                Logger.Info($"Importing TMDb movies from list: {Settings.ListId}&page={pageNumber}");

                var requestBuilder = RequestBuilder.Create()
                                                   .SetSegment("api", "3")
                                                   .SetSegment("route", "list")
                                                   .SetSegment("id", Settings.ListId)
                                                   .SetSegment("secondaryRoute", "");

                requestBuilder.AddQueryParam("page", pageNumber, true);

                yield return new ImportListRequest(requestBuilder.Accept(HttpAccept.Json)
                                                                .Build());
            }
        }
    }
}
