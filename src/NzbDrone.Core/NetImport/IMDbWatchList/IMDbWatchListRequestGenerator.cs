using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.NetImport.IMDbWatchList
{
    public class IMDbWatchListRequestGenerator : INetImportRequestGenerator
    {
        public IMDbWatchListSettings Settings { get; set; }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var pageableRequests = new NetImportPageableRequestChain();

            pageableRequests.Add(GetMovies(null));

            return pageableRequests;
        }

        public NetImportPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            return new NetImportPageableRequestChain();
        }

        private IEnumerable<NetImportRequest> GetMovies(string searchParameters)
        {
            var request = new NetImportRequest($"{Settings.Link.Trim()}", HttpAccept.Rss);
            yield return request;
        }
    }
}
