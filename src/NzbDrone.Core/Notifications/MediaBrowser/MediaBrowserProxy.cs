using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Notifications.MediaBrowser.Model;

namespace NzbDrone.Core.Notifications.Emby
{
    public class MediaBrowserProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public MediaBrowserProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void Notify(MediaBrowserSettings settings, string title, string message)
        {
            var path = "/Notifications/Admin";
            var request = BuildRequest(path, settings);
            request.Headers.ContentType = "application/json";
            request.Method = HttpMethod.POST;

            request.SetContent(new
            {
                Name = title,
                Description = message,
                ImageUrl = "https://raw.github.com/lidarr/Lidarr/develop/Logo/64.png"
            }.ToJson());

            ProcessRequest(request, settings);
        }

        public void Update(MediaBrowserSettings settings, List<string> musicCollectionPaths)
        {
            string path;
            HttpRequest request;

            if (musicCollectionPaths.Any())
            {
                path = "/Library/Media/Updated";
                request = BuildRequest(path, settings);
                request.Headers.ContentType = "application/json";

                var updateInfo = new List<EmbyMediaUpdateInfo>();

                foreach (var colPath in musicCollectionPaths)
                {
                    updateInfo.Add(new EmbyMediaUpdateInfo
                    {
                        Path = colPath,
                        UpdateType = "Created"
                    });
                }

                request.SetContent(new
                {
                    Updates = updateInfo
                }.ToJson());
            }
            else
            {
                path = "/Library/Refresh";
                request = BuildRequest(path, settings);
            }

            request.Method = HttpMethod.POST;

            ProcessRequest(request, settings);
        }

        private string ProcessRequest(HttpRequest request, MediaBrowserSettings settings)
        {
            request.Headers.Add("X-MediaBrowser-Token", settings.ApiKey);

            var response = _httpClient.Execute(request);

            _logger.Trace("Response: {0}", response.Content);

            CheckForError(response);

            return response.Content;
        }

        private HttpRequest BuildRequest(string path, MediaBrowserSettings settings)
        {
            var scheme = settings.UseSsl ? "https" : "http";
            var url = $@"{scheme}://{settings.Address}/mediabrowser";

            return new HttpRequestBuilder(url).Resource(path).Build();
        }

        private void CheckForError(HttpResponse response)
        {
            _logger.Debug("Looking for error in response: {0}", response);

            //TODO: actually check for the error
        }

        public List<EmbyMediaFolder> GetArtist(MediaBrowserSettings settings)
        {
            var path = "/Library/MediaFolders";
            var request = BuildRequest(path, settings);
            request.Method = HttpMethod.GET;

            var response = ProcessRequest(request, settings);

            return Json.Deserialize<EmbyMediaFoldersResponse>(response).Items;
        }
    }
}
