using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.CouchPotato
{
    public class CouchPotatoRequestGenerator : IImportListRequestGenerator
    {
        public CouchPotatoSettings Settings { get; set; }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetMovies(null));

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMovies(string searchParameters)
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

            var request = new ImportListRequest($"{Settings.Link.Trim()}:{Settings.Port}{urlBase}/api/{Settings.ApiKey}/movie.list/{status}", HttpAccept.Json);
            yield return request;
        }
    }
}
