using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Rest;
using RestSharp;
using System.IO;
using System.Xml.Linq;

namespace NzbDrone.Core.Notifications.Subsonic
{
    public interface ISubsonicServerProxy
    {
        string GetBaseUrl(SubsonicSettings settings, string relativePath = null);
        void Notify(SubsonicSettings settings, string message);
        void Update(SubsonicSettings settings);
        string Version(SubsonicSettings settings);
    }

    public class SubsonicServerProxy : ISubsonicServerProxy
    {
        private readonly Logger _logger;

        public SubsonicServerProxy(Logger logger)
        {
            _logger = logger;
        }

        public string GetBaseUrl(SubsonicSettings settings, string relativePath = null)
        {
            var baseUrl = HttpRequestBuilder.BuildBaseUrl(settings.UseSsl, settings.Host, settings.Port, settings.UrlBase);
            baseUrl = HttpUri.CombinePath(baseUrl, relativePath);

            return baseUrl;
        }

        public void Notify(SubsonicSettings settings, string message)
        {
            var resource = "addChatMessage";
            var request = GetSubsonicServerRequest(resource, Method.GET, settings);
            request.AddParameter("message", message);
            var client = GetSubsonicServerClient(settings);
            var response = client.Execute(request);

            _logger.Trace("Update response: {0}", response.Content);
            CheckForError(response, settings);
        }

        public void Update(SubsonicSettings settings)
        {
            var resource = "startScan";
            var request = GetSubsonicServerRequest(resource, Method.GET, settings);
            var client = GetSubsonicServerClient(settings);
            var response = client.Execute(request);

            _logger.Trace("Update response: {0}", response.Content);
            CheckForError(response, settings);
        }

        public string Version(SubsonicSettings settings)
        {
            var request = GetSubsonicServerRequest("ping", Method.GET, settings);
            var client = GetSubsonicServerClient(settings);
            var response = client.Execute(request);

            _logger.Trace("Version response: {0}", response.Content);
            CheckForError(response, settings);
            
            var xDoc = XDocument.Load(new StringReader(response.Content.Replace("&", "&amp;")));
            var version = xDoc.Root?.Attribute("version")?.Value;

            if (version == null)
            {
                throw new SubsonicException("Could not read version from Subsonic");
            }

            return version;
        }

        private RestClient GetSubsonicServerClient(SubsonicSettings settings)
        {
            return RestClientFactory.BuildClient(GetBaseUrl(settings, "rest"));
        }

        private RestRequest GetSubsonicServerRequest(string resource, Method method, SubsonicSettings settings)
        {
            var request = new RestRequest(resource, method);

            if (settings.Username.IsNotNullOrWhiteSpace())
            {
                request.AddParameter("u", settings.Username);
                request.AddParameter("p", settings.Password);
                request.AddParameter("c", "Lidarr");
                request.AddParameter("v", "1.15.0");
            }

            return request;
        }

        private void CheckForError(IRestResponse response, SubsonicSettings settings)
        {
            _logger.Trace("Checking for error");

            var xDoc = XDocument.Load(new StringReader(response.Content.Replace("&", "&amp;")));
            var status = xDoc.Root?.Attribute("status")?.Value;

            if (status == null)
            {
                throw new SubsonicException("Invalid Response, Check Server Settings");
            }

            if (status == "failed")
            {
                var ns = xDoc.Root.GetDefaultNamespace();
                var error = xDoc.Root.Element(XName.Get("error", ns.ToString()));
                var errorMessage = error?.Attribute("message")?.Value;
                var errorCode = error?.Attribute("code")?.Value;

                if (errorCode == null)
                {
                    throw new SubsonicException("Subsonic returned error, check settings");
                }

                if (errorCode == "40")
                {
                    throw new SubsonicAuthenticationException(errorMessage);
                }

                throw new SubsonicException(errorMessage);

            }

            if (response.Content.IsNullOrWhiteSpace())
            {
                _logger.Trace("No response body returned, no error detected");
                return;
            }

            _logger.Trace("No error detected");
        }
    }
}
