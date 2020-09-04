﻿using System.Collections.Generic;
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

        public TMDbUserRequestGenerator()
        {
            MaxPages = 3;
        }

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

            requestBuilder.Method = HttpMethod.GET;

            yield return new ImportListRequest(requestBuilder.Build());
        }
    }
}
