using System.Collections.Generic;
using System.Net;
using Lidarr.Api.V1.Artist;
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
            var request = BuildRequest("lookup");
            request.AddQueryParameter("term", term);
            return Get<List<ArtistResource>>(request);
        }

        public List<ArtistResource> Editor(ArtistEditorResource artist)
        {
            var request = BuildRequest("editor");
            request.AddJsonBody(artist);
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
