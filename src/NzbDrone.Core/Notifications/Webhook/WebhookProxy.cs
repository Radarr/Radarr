using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Rest;
using System;
using System.Text;

namespace NzbDrone.Core.Notifications.Webhook
{
    public interface IWebhookProxy
    {
        void SendWebhook(WebhookPayload payload, WebhookSettings settings);
    }

    class WebhookProxy : IWebhookProxy
    {
        private readonly IHttpClient _httpClient;

        public WebhookProxy(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SendWebhook(WebhookPayload body, WebhookSettings settings)
        {
            try
            {
                var request = new HttpRequestBuilder(settings.Url)
                    .Accept(HttpAccept.Json)
                    .Build();

                request.Method = (HttpMethod)settings.Method;
                request.Headers.ContentType = "application/json";
                request.SetContent(body.ToJson());

                if (!String.IsNullOrEmpty(settings.Username) || !String.IsNullOrEmpty(settings.Password))
                {
                    var authInfo = settings.Username + ":" + settings.Password;
                    authInfo = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(authInfo));
                    request.Headers.Set("Authorization", "Basic " + authInfo);
                }

                _httpClient.Execute(request);
            }
            catch (RestException ex)
            {
                throw new WebhookException("Unable to post to webhook: {0}", ex, ex.Message);
            }
        }
    }
}
