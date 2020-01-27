using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.NetImport.CouchPotato
{
    public class CouchPotatoRequestGenerator : INetImportRequestGenerator
    {
        public CouchPotatoSettings Settings { get; set; }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var pageableRequests = new NetImportPageableRequestChain();

            pageableRequests.Add(GetMovies(null));

            return pageableRequests;
        }

        private IEnumerable<NetImportRequest> GetMovies(string searchParameters)
        {
            var urlBase = "";
            if (!string.IsNullOrWhiteSpace(Settings.UrlBase))
            {
                urlBase = Settings.UrlBase.StartsWith("/") ? Settings.UrlBase : $"/{Settings.UrlBase}";
            }

            var status = "";

            if (Settings.OnlyActive)
            {
                status = "?status=active";
            }

            var request = new NetImportRequest($"{Settings.Link.Trim()}:{Settings.Port}{urlBase}/api/{Settings.ApiKey}/movie.list/{status}", HttpAccept.Json);
            yield return request;
        }
    }
}
