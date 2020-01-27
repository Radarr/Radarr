﻿using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.NetImport.TMDb.Person
{
    public class TMDbPersonRequestGenerator : INetImportRequestGenerator
    {
        public TMDbPersonSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public IHttpRequestBuilderFactory RequestBuilder { get; set; }
        public Logger Logger { get; set; }

        public TMDbPersonRequestGenerator()
        {
        }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var pageableRequests = new NetImportPageableRequestChain();

            pageableRequests.Add(GetMoviesRequest());

            return pageableRequests;
        }

        private IEnumerable<NetImportRequest> GetMoviesRequest()
        {
            Logger.Info($"Importing TMDb movies from person: {Settings.PersonId}");

            var requestBuilder = RequestBuilder.Create()
                                               .SetSegment("api", "3")
                                               .SetSegment("route", "person")
                                               .SetSegment("id", Settings.PersonId)
                                               .SetSegment("secondaryRoute", "/movie_credits");

            yield return new NetImportRequest(requestBuilder.Accept(HttpAccept.Json)
                                                            .Build());
        }
    }
}
