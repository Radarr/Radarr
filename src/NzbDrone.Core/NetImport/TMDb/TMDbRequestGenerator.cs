using System;
using NzbDrone.Common.Http;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;

namespace NzbDrone.Core.NetImport.TMDb
{
    public class TMDbRequestGenerator : INetImportRequestGenerator
    {
        public TMDbSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public Logger Logger { get; set; }

        public int MaxPages { get; set; }

        public TMDbRequestGenerator()
        {
            MaxPages = 5;
        }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var searchType = "";

            switch (Settings.ListType)
            {
                case (int)TMDbListType.Theaters:
                    searchType = "/3/movie/now_playing";
                    break;
                case (int)TMDbListType.Popular:
                    searchType = "/3/movie/popular";
                    break;
                case (int)TMDbListType.Top:
                    searchType = "/3/movie/top_rated";
                    break;
                case (int)TMDbListType.Upcoming:
                    searchType = "/3/movie/upcoming";
                    break;
            }

            // First query to get the total_Pages
            var requestBuilder = new HttpRequestBuilder($"{Settings.Link.Trim()}")
            {
                LogResponseContent = true
            };

            requestBuilder.Method = HttpMethod.GET;
            requestBuilder.Resource(searchType);

            var request = requestBuilder
                .AddQueryParam("api_key", "1a7373301961d03f97f853a876dd1212")
                .Accept(HttpAccept.Json)
                .Build();

            var response = HttpClient.Execute(request);
            var result = Json.Deserialize<MovieSearchRoot>(response.Content);

            // @TODO Prolly some error handling to do here

            var totalPages = result.total_pages;
            var totalResults = result.total_results;

            // Now let it be done!
            var pageableRequests = new NetImportPageableRequestChain();
            pageableRequests.Add(GetPagedRequests(searchType, totalPages, totalResults));
            return pageableRequests;
        } 
        
        private IEnumerable<NetImportRequest> GetPagedRequests(string searchType, int totalPages, int totalResults)
        {
            var baseUrl = $"{Settings.Link.Trim()}{searchType}?api_key=1a7373301961d03f97f853a876dd1212";
            for (var pageNumber = 1; pageNumber < totalPages; pageNumber++)
            {
                // Limit the amount of pages
                if (pageNumber >= MaxPages)
                {
                    Logger.Info($"Found more than {MaxPages} pages");
                    break;
                }

                Console.WriteLine($"{baseUrl}&page={pageNumber}");
                yield return new NetImportRequest($"{baseUrl}&page={pageNumber}", HttpAccept.Json);
            }
        }
    }
}
