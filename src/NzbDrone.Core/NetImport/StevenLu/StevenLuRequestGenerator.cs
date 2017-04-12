using NzbDrone.Common.Http;
using System.Collections.Generic;

namespace NzbDrone.Core.NetImport.StevenLu
{
    public class StevenLuRequestGenerator : INetImportRequestGenerator
    {
        public StevenLuSettings Settings { get; set; }

        public virtual void Clean(NzbDrone.Core.Tv.Movie movie)
        {
            ;
        }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var pageableRequests = new NetImportPageableRequestChain();
            pageableRequests.Add(GetMovies(null));
            return pageableRequests;
        }

        private IEnumerable<NetImportRequest> GetMovies(string searchParameters)
        {
            var request = new NetImportRequest($"{Settings.Link.Trim()}", HttpAccept.Json);
            yield return request;
        }
    }
}
