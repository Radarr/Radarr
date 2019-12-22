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

            pageableRequests.Add(GetMovies(null));

            return pageableRequests;
        }

        //public NetImportPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        //{
        //    return new NetImportPageableRequestChain();
        //}
        private IEnumerable<NetImportRequest> GetMovies(string searchParameters)
        {
            var request = new NetImportRequest($"{Settings.Link.Trim()}", HttpAccept.Rss);
            yield return request;
        }
    }
}
