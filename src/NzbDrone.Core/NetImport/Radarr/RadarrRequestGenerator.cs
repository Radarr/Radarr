using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.NetImport.Radarr
{
    public class RadarrRequestGenerator : INetImportRequestGenerator
    {
        public RadarrSettings Settings { get; set; }
        public IHttpClient HttpClient { get; set; }
        public Logger Logger { get; set; }

        public RadarrRequestGenerator()
        {
        }

        public virtual NetImportPageableRequestChain GetMovies()
        {
            var pageableRequests = new NetImportPageableRequestChain();

            var baseUrl = Settings.BaseUrl.TrimEnd('/');

            var request = new NetImportRequest($"{baseUrl}/api/v3/movie", HttpAccept.Json);

            request.HttpRequest.Headers["X-Api-Key"] = Settings.ApiKey;

            request.HttpRequest.SuppressHttpError = true;

            pageableRequests.Add(new List<NetImportRequest> { request });

            return pageableRequests;
        }
    }
}
