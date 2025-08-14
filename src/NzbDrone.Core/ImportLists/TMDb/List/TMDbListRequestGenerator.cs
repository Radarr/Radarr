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

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetMoviesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMoviesRequest()
        {
            Logger.Info("Importing TMDb movies from list: {0}", Settings.ListId);

            var requestBuilder = RequestBuilder.Create()
                .SetSegment("api", "4")
                .SetSegment("route", "list")
                .SetSegment("id", Settings.ListId)
                .SetSegment("secondaryRoute", "");

            Logger.Debug("Getting total pages for TMDb List: {0}", Settings.ListId);

            var jsonResponse = JsonConvert.DeserializeObject<MovieSearchResource>(HttpClient.Execute(requestBuilder.Build()).Content);

            MaxPages = jsonResponse.TotalPages;

            if (jsonResponse.TotalPages > 1)
            {
                Logger.Debug("TMDb List {0}: processing {1} pages", Settings.ListId, MaxPages);
            }

            for (var pageNumber = 1; pageNumber <= MaxPages; pageNumber++)
            {
                requestBuilder.AddQueryParam("page", pageNumber, true);

                var request = requestBuilder.Build();

                if (pageNumber == 1 || pageNumber == MaxPages)
                {
                    Logger.Debug("Processing TMDb List page {0} of {1}", pageNumber, MaxPages);
                }

                yield return new ImportListRequest(request);
            }
        }
    }
}
