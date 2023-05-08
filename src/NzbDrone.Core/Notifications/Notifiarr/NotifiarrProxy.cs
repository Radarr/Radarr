using System.Net.Http;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Notifications.Webhook;

namespace NzbDrone.Core.Notifications.Notifiarr
{
    public interface INotifiarrProxy
    {
        void SendNotification(WebhookPayload payload, NotifiarrSettings settings);
    }

    public class NotifiarrProxy : INotifiarrProxy
    {
        private const string URL = "https://notifiarr.com";
        private readonly IHttpClient _httpClient;

        public NotifiarrProxy(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SendNotification(WebhookPayload payload, NotifiarrSettings settings)
        {
            ProcessNotification(payload, settings);
        }

        private void ProcessNotification(WebhookPayload payload, NotifiarrSettings settings)
        {
            try
            {
                var request = new HttpRequestBuilder(URL + "/api/v1/notification/radarr")
                    .Accept(HttpAccept.Json)
                    .SetHeader("X-API-Key", settings.APIKey)
                    .Build();

                request.Method = HttpMethod.Post;

                request.Headers.ContentType = "application/json";
                request.SetContent(payload.ToJson());

                _httpClient.Post(request);
            }
            catch (HttpException ex)
            {
                var responseCode = ex.Response.StatusCode;
                switch ((int)responseCode)
                {
                    case 401:
                        throw new NotifiarrException("API key is invalid");
                    case 400:
                        throw new NotifiarrException("Unable to send notification. Ensure Radarr Integration is enabled & assigned a channel on Notifiarr");
                    case 502:
                    case 503:
                    case 504:
                        throw new NotifiarrException("Unable to send notification. Service Unavailable", ex);
                    case 520:
                    case 521:
                    case 522:
                    case 523:
                    case 524:
                        throw new NotifiarrException("Cloudflare Related HTTP Error - Unable to send notification", ex);
                    default:
                        throw new NotifiarrException("Unknown HTTP Error - Unable to send notification", ex);
                }
            }
        }
    }
}
