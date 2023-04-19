using System;
using System.Net;
using System.Text;
using System.Web;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Notifications.Signal
{
    public interface ISignalProxy
    {
        void SendNotification(string title, string message, SignalSettings settings);
        ValidationFailure Test(SignalSettings settings);
    }

    public class SignalProxy : ISignalProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public SignalProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void SendNotification(string title, string message, SignalSettings settings)
        {
            var text = $"{HttpUtility.HtmlEncode(title)}\n{HttpUtility.HtmlEncode(message)}";

            var urlSignalAPI = HttpRequestBuilder.BuildBaseUrl(
                settings.UseSSLSignalAPI,
                settings.SignalAPIHost,
                settings.SignalAPIPort,
                "/v2/send");

            var requestBuilder = new HttpRequestBuilder(urlSignalAPI).Post();
            var request = requestBuilder.Build();

            if (settings.AuthenticationSignalAPI)
            {
                var authCredentials = $"Basic {System.Convert.ToBase64String(Encoding.UTF8.GetBytes($"{settings.LoginSignalAPI}:{settings.PasswordSignalAPI}"))}";
                request = requestBuilder.SetHeader("Authorization", authCredentials)
                                            .Build();
            }

            request.Headers.ContentType = "application/json";

            var payload = new SignalPayload
            {
                Message = text,
                Number = settings.SourceNumber,
                Recipients = new string[1]
            };
            payload.Recipients[0] = settings.ReceiverID;
            request.SetContent(payload.ToJson());
            _httpClient.Post(request);
        }

        public ValidationFailure Test(SignalSettings settings)
        {
            try
            {
                const string title = "Test Notification";
                const string body = "This is a test message from Radarr";

                SendNotification(title, body, settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");

                if (ex is WebException webException)
                {
                    return new ValidationFailure("Connection", $"{webException.Status.ToString()}: {webException.Message}");
                }
                else if (ex is Common.Http.HttpException restException && restException.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var error = Json.Deserialize<SignalError>(restException.Response.Content);
                    var property = error.Description.ContainsIgnoreCase("chat not found") ? "ChatId" : "BotToken";

                    return new ValidationFailure(property, error.Description);
                }

                return new ValidationFailure("BotToken", "Unable to send test message");
            }

            return null;
        }
    }
}
