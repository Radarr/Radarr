using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.NetImport.Kitsu
{
    public class KitsuRequestGenerator : INetImportRequestGenerator
    {
        public KitsuSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public Logger Logger { get; set; }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var baseUrl = "https://kitsu.io/api/edge";
            var userUrl = $"/users?filter[name]={Settings.Username}";

            var userRequestBuilder = new HttpRequestBuilder(baseUrl);

            userRequestBuilder.Method = HttpMethod.GET;
            userRequestBuilder.Resource(userUrl);

            var userRequest = userRequestBuilder
                .Accept(HttpAccept.JsonApi)
                .Build();

            var userResponse = HttpClient.Execute(userRequest);
            var userResult = Json.Deserialize<KitsuUserResponse>(userResponse.Content);
            var userId = userResult.data[0].id;

            var libraryUrl = $"/library-entries?filter[userId]={userId}&filter[kind]=anime&filter[status]={Settings.List}&include=anime";

            var requestBuilder = new HttpRequestBuilder(baseUrl);

            requestBuilder.Method = HttpMethod.GET;
            requestBuilder.Resource($"{libraryUrl}&page[limit]=1");

            var request = requestBuilder
                .Accept(HttpAccept.JsonApi)
                .Build();

            var response = HttpClient.Execute(request);
            var result = Json.Deserialize<KitsuResponse>(response.Content);

            var pageableRequests = new NetImportPageableRequestChain();
            pageableRequests.Add(GetPagedRequests(baseUrl + libraryUrl, result.meta.count));

            return pageableRequests;
        }

        private IEnumerable<NetImportRequest> GetPagedRequests(string url, int count)
        {
            var pageLimit = 50;
            for (var pageOffset = 0; pageOffset <= count; pageOffset += pageLimit)
            {
                Logger.Trace($"Importing Kitsu movies from: {url}&page[limit]={pageLimit}&page[offset]={pageOffset}");
                yield return new NetImportRequest($"{url}&page[limit]={pageLimit}&page[offset]={pageOffset}", HttpAccept.JsonApi);
            }
        }
    }
}
