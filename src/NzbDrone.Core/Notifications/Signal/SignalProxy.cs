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
            var text = new StringBuilder();
            text.Append(title);
            text.Append(new string("\n"));
            text.Append(message);

            var urlSignalAPI = HttpRequestBuilder.BuildBaseUrl(
                settings.UseSSL,
                settings.Host,
                settings.Port,
                "/v2/send");

            var requestBuilder = new HttpRequestBuilder(urlSignalAPI).Post();

            if (settings.LoginSignalAPI.IsNotNullOrWhiteSpace() && settings.PasswordSignalAPI.IsNotNullOrWhiteSpace())
            {
                requestBuilder.NetworkCredential = new BasicNetworkCredential(settings.LoginSignalAPI, settings.PasswordSignalAPI);
            }

            var request = requestBuilder.Build();

            request.Headers.ContentType = "application/json";

            var payload = new SignalPayload
            {
                Message = text.ToString(),
                Number = settings.SourceNumber,
                Recipients = new[] { settings.ReceiverID }
            };
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
                    return new ValidationFailure("Host", $"{webException.Status.ToString()}: {webException.Message}");
                }
                else if (ex is Common.Http.HttpException restException)
                {
                    if (restException.Response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var error = Json.Deserialize<SignalError>(restException.Response.Content);
                        var property = "Host";

                        if (error.Error.ContainsIgnoreCase("Invalid group id"))
                        {
                            property = "ReceiverID";
                        }
                        else if (error.Error.ContainsIgnoreCase("Invalid account"))
                        {
                            property = "SourceNumber";
                        }

                        return new ValidationFailure(property, error.Error);
                    }
                    else if (restException.Response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        var property = "LoginSignalAPI";
                        return new ValidationFailure(property, "Login/Password invalid");
                    }
                }

                return new ValidationFailure("Host", "Unable to send test message");
            }

            return null;
        }
    }
}
