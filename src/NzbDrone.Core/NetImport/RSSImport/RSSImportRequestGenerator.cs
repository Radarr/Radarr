using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.NetImport.RSSImport
{
    public class RSSImportRequestGenerator : INetImportRequestGenerator
    {
        public RSSImportSettings Settings { get; set; }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var pageableRequests = new NetImportPageableRequestChain();
            pageableRequests.Add(GetRssMovies());
            return pageableRequests;
        }

        private IEnumerable<NetImportRequest> GetRssMovies()
        {
            var request = new NetImportRequest($"{Settings.Link.Trim()}", HttpAccept.Rss);
            yield return request;
        }
    }
}
