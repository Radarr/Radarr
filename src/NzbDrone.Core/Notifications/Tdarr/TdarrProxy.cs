using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Notifications.Tdarr
{
    public interface ITdarrProxy
    {
        void ScanFile(string path, TdarrSettings settings);
        void Test(TdarrSettings settings);
    }

    public class TdarrProxy : ITdarrProxy
    {
        private readonly IHttpClient _httpClient;

        public TdarrProxy(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void ScanFile(string path, TdarrSettings settings)
        {
            var request = BuildRequest("/api/v2/scan-files", settings).Post().Build();
            request.Headers.ContentType = "application/json";
            request.SetContent(new
            {
                data = new
                {
                    scanConfig = new
                    {
                        dbID = settings.LibraryId,
                        arrayOrPath = new[] { path },
                        mode = "scanFolderWatcher"
                    }
                }
            }.ToJson());

            _httpClient.Execute(request);
        }

        public void Test(TdarrSettings settings)
        {
            var request = BuildRequest("/api/v2/status", settings).Build();
            _httpClient.Execute(request);
        }

        private HttpRequestBuilder BuildRequest(string resource, TdarrSettings settings)
        {
            var scheme = settings.UseSsl ? "https" : "http";
            var builder = new HttpRequestBuilder($@"{scheme}://{settings.Address}")
                .Resource(resource);

            if (settings.ApiKey.IsNotNullOrWhiteSpace())
            {
                builder.SetHeader("x-api-key", settings.ApiKey);
            }

            return builder;
        }
    }
}
