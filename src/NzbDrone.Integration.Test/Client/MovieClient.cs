using System.Collections.Generic;
using System.Net;
using Radarr.Api.V3.Movies;
using RestSharp;

namespace NzbDrone.Integration.Test.Client
{
    public class MovieClient : ClientBase<MovieResource>
    {
        public MovieClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }

        public List<MovieResource> Lookup(string term)
        {
            var request = BuildRequest("lookup");
            request.AddQueryParameter("term", term);
            return Get<List<MovieResource>>(request);
        }

        public List<MovieResource> Editor(MovieEditorResource movie)
        {
            var request = BuildRequest("editor");
            request.AddJsonBody(movie);
            return Put<List<MovieResource>>(request);
        }

        public MovieResource Get(string slug, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var request = BuildRequest(slug);
            return Get<MovieResource>(request, statusCode);
        }

    }

    public class SystemInfoClient : ClientBase<MovieResource>
    {
        public SystemInfoClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }
    }
}
