using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.TMDb.User
{
    public class TMDbUserRequestGenerator : IImportListRequestGenerator
    {
        public TMDbUserSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public IHttpRequestBuilderFactory RequestBuilder { get; set; }
        public Logger Logger { get; set; }

        public int MaxPages { get; set; }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetMoviesRequests());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMoviesRequests()
        {
            var requestBuilder = RequestBuilder.Create()
                .SetHeader("Authorization", $"Bearer {Settings.AccessToken}")
                .SetSegment("api", "4")
                .SetSegment("route", "account")
                .SetSegment("id", Settings.AccountId);

            switch (Settings.ListType)
            {
                case (int)TMDbUserListType.Watchlist:
                    requestBuilder.SetSegment("secondaryRoute", "/movie/watchlist");
                    break;
                case (int)TMDbUserListType.Recommendations:
                    requestBuilder.SetSegment("secondaryRoute", "/movie/recommendations");
                    break;
                case (int)TMDbUserListType.Rated:
                    requestBuilder.SetSegment("secondaryRoute", "/movie/rated");
                    break;
                case (int)TMDbUserListType.Favorite:
                    requestBuilder.SetSegment("secondaryRoute", "/movie/favorites");
                    break;
            }

            requestBuilder.Accept(HttpAccept.Json);

            requestBuilder.Method = HttpMethod.Get;

            Logger.Trace("Getting total pages for TMDb User {0}", (TMDbUserListType)Settings.ListType);

            var jsonResponse = JsonConvert.DeserializeObject<MovieSearchResource>(HttpClient.Execute(requestBuilder.Build()).Content);

            MaxPages = jsonResponse.TotalPages;

            if (jsonResponse.TotalPages > 1)
            {
                Logger.Debug("TMDb User {0}: processing {1} pages", (TMDbUserListType)Settings.ListType, MaxPages);
            }

            for (var pageNumber = 1; pageNumber <= MaxPages; pageNumber++)
            {
                requestBuilder.AddQueryParam("page", pageNumber, true);

                var request = requestBuilder.Build();

                if (pageNumber == 1 || pageNumber == MaxPages)
                {
                    Logger.Debug("Processing TMDb User page {0} of {1}", pageNumber, MaxPages);
                }

                yield return new ImportListRequest(request);
            }
        }
    }
}
