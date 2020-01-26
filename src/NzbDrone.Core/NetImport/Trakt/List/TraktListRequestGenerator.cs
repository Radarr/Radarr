using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.NetImport.Trakt.List
{
    public class TraktListRequestGenerator : INetImportRequestGenerator
    {
        public TraktListSettings Settings { get; set; }

        public TraktListRequestGenerator()
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
            var link = Settings.Link.Trim();

            var listName = Parser.Parser.ToUrlSlug(Settings.Listname.Trim());
            link += $"/users/{Settings.Username.Trim()}/lists/{listName}/items/movies?limit={Settings.Limit}";

            var request = new NetImportRequest($"{link}", HttpAccept.Json);

            request.HttpRequest.Headers.Add("trakt-api-version", "2");
            request.HttpRequest.Headers.Add("trakt-api-key", Settings.ClientId); //aeon

            if (Settings.AccessToken.IsNotNullOrWhiteSpace())
            {
                request.HttpRequest.Headers.Add("Authorization", "Bearer " + Settings.AccessToken);
            }

            yield return request;
        }
    }
}
