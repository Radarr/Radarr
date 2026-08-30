using System.Collections.Generic;
using System.Net;
using Radarr.Api.V3.Movies;
using Radarr.Http;
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

        public PagingResource<MovieResource> Page(int page, int pageSize = 100, string sortKey = "sortTitle", string sortDirection = "ascending")
        {
            var request = BuildRequest("page");
            request.AddQueryParameter("page", page.ToString());
            request.AddQueryParameter("pageSize", pageSize.ToString());
            request.AddQueryParameter("sortKey", sortKey);
            request.AddQueryParameter("sortDirection", sortDirection);
            return Get<PagingResource<MovieResource>>(request);
        }

        public List<MovieResource> SearchExisting(string term)
        {
            var request = BuildRequest("search");
            request.AddQueryParameter("term", term);
            return Get<List<MovieResource>>(request);
        }

        public MovieDetailsResource GetBySlug(string slug)
        {
            var request = BuildRequest($"slug/{slug}");
            return Get<MovieDetailsResource>(request);
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
