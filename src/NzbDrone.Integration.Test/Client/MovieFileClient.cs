using System.Collections.Generic;
using System.Net;
using Radarr.Api.V3.MovieFiles;
using Radarr.Api.V3.Movies;
using RestSharp;

namespace NzbDrone.Integration.Test.Client
{
    public class MovieFileClient : ClientBase<MovieFileResource>
    {
        public MovieFileClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }

        public MovieFileResource Get(string slug, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var request = BuildRequest(slug);
            return Get<MovieFileResource>(request, statusCode);
        }
    }
}
