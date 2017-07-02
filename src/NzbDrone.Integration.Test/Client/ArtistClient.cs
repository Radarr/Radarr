using System.Collections.Generic;
using System.Net;
using NzbDrone.Api.Music;
using RestSharp;

namespace NzbDrone.Integration.Test.Client
{
    public class ArtistClient : ClientBase<ArtistResource>
    {
        public ArtistClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }

        public List<ArtistResource> Lookup(string term)
        {
            var request = BuildRequest("lookup?term={term}");
            request.AddUrlSegment("term", term);
            return Get<List<ArtistResource>>(request);
        }

        public List<ArtistResource> Editor(List<ArtistResource> series)
        {
            var request = BuildRequest("editor");
            request.AddBody(series);
            return Put<List<ArtistResource>>(request);
        }

        public ArtistResource Get(string slug, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var request = BuildRequest(slug);
            return Get<ArtistResource>(request, statusCode);
        }

    }

    public class SystemInfoClient : ClientBase<ArtistResource>
    {
        public SystemInfoClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }
    }
}
