﻿using System.Collections.Generic;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.TMDb.Company
{
    public class TMDbCompanyRequestGenerator : IImportListRequestGenerator
    {
        public TMDbCompanySettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public IHttpRequestBuilderFactory RequestBuilder { get; set; }
        public Logger Logger { get; set; }
        public int MaxPages { get; set; }

        public TMDbCompanyRequestGenerator()
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
            Logger.Info($"Importing TMDb movies from company: {Settings.CompanyId}");

            var requestBuilder = RequestBuilder.Create()
                                               .SetSegment("api", "3")
                                               .SetSegment("route", "discover")
                                               .SetSegment("id", $"movie")
                                               .SetSegment("secondaryRoute", "");

            requestBuilder.AddQueryParam("with_companies", Settings.CompanyId);

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
