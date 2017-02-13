﻿using System;
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
            MaxPages = 3;
        }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var searchType = "";

            switch (Settings.ListType)
            {
                case (int)TMDbListType.List:
                    searchType = $"/3/list/{Settings.ListId}";
                    break;
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

            var pageableRequests = new NetImportPageableRequestChain();
            if (Settings.ListType != (int) TMDbListType.List)
            {
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
                pageableRequests.Add(GetPagedRequests(searchType, result.total_pages));
                return pageableRequests;
            }
            else
            {
                pageableRequests.Add(GetPagedRequests(searchType, 0));
                return pageableRequests;
            }
        } 
        
        private IEnumerable<NetImportRequest> GetPagedRequests(string searchType, int totalPages)
        {
            var baseUrl = $"{Settings.Link.Trim()}{searchType}?api_key=1a7373301961d03f97f853a876dd1212";
            if (Settings.ListType != (int) TMDbListType.List)
            {
                for (var pageNumber = 1; pageNumber <= totalPages; pageNumber++)
                {
                    // Limit the amount of pages
                    if (pageNumber >= MaxPages + 1)
                    {
                        Logger.Info(
                            $"Found more than {MaxPages} pages, skipping the {totalPages - (MaxPages + 1)} remaining pages");
                        break;
                    }

                    Logger.Trace($"Importing TMDb movies from: {baseUrl}&page={pageNumber}");
                    yield return new NetImportRequest($"{baseUrl}&page={pageNumber}", HttpAccept.Json);
                }
            }
            else
            {
                Logger.Trace($"Importing TMDb movies from: {baseUrl}");
                yield return new NetImportRequest($"{baseUrl}", HttpAccept.Json);
            }

        }
    }
}
