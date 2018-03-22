using System.Collections.Generic;
using System.Net;
using NzbDrone.Api.Movies;
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
            var request = BuildRequest("lookup?term={term}");
            request.AddUrlSegment("term", term);
            return Get<List<MovieResource>>(request);
        }

        public List<MovieResource> Editor(List<MovieResource> movie)
        {
            var request = BuildRequest("editor");
            request.AddBody(movie);
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
