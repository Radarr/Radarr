using System.Collections.Generic;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.NetImport.StevenLu
{
    public class StevenLuRequestGenerator : INetImportRequestGenerator
    {
        public StevenLuSettings Settings { get; set; }

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
