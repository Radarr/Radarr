using System.Collections.Generic;
using Newtonsoft.Json;
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
        public int MaxPages { get; set; }

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
                                               .SetSegment("api", "4")
                                               .SetSegment("route", "list")
                                               .SetSegment("id", Settings.ListId)
                                               .SetSegment("secondaryRoute", "");

            Logger.Debug($"Getting total pages that TMDb List: {Settings.ListId} consists of");

            var jsonResponse = JsonConvert.DeserializeObject<MovieSearchResource>(HttpClient.Execute(requestBuilder.Build()).Content);

            MaxPages = jsonResponse.TotalPages;

            for (var pageNumber = 1; pageNumber <= MaxPages; pageNumber++)
            {
                requestBuilder.AddQueryParam("page", pageNumber, true);

                var request = requestBuilder.Build();

                Logger.Debug($"Importing TMDb movies from: {request.Url}");

                yield return new ImportListRequest(request);
            }
        }
    }
}
