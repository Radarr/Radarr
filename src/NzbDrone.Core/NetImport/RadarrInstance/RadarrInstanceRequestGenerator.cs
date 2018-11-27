using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.NetImport.RadarrInstance
{
    public class RadarrInstanceRequestGenerator : INetImportRequestGenerator
    {
        public RadarrInstanceSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public Logger Logger { get; set; }

        public int MaxPages { get; set; }

        public RadarrInstanceRequestGenerator()
        {
            MaxPages = 1;
        }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var pageableRequests = new NetImportPageableRequestChain();

            var baseUrl = $"{Settings.URL.TrimEnd('/')}";

            var request = new NetImportRequest($"{baseUrl}/api/movie", HttpAccept.Json);

            request.HttpRequest.Headers["X-Api-Key"] = Settings.APIKey;
            
            request.HttpRequest.SuppressHttpError = true;

            pageableRequests.Add(new List<NetImportRequest> { request });
            return pageableRequests;
        }
    }
}
