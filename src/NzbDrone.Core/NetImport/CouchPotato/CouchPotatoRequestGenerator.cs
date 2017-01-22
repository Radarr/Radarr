using NzbDrone.Common.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            var request = new NetImportRequest($"{Settings.Link.Trim()}:{Settings.Port}/api/{Settings.ApiKey}/movie.list/?status=active", HttpAccept.Json);
            yield return request;
        }
    }
}
