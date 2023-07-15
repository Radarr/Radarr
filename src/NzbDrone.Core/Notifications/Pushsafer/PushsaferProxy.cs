using System;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Notifications.Pushsafer
{
    public interface IPushsaferProxy
    {
        void SendNotification(string title, string message, PushsaferSettings settings);
        ValidationFailure Test(PushsaferSettings settings);
    }

    public class PushsaferProxy : IPushsaferProxy
    {
        private const string URL = "https://pushsafer.com/api";
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public PushsaferProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void SendNotification(string title, string message, PushsaferSettings settings)
        {
            var requestBuilder = new HttpRequestBuilder(URL).Post();

            requestBuilder.AddFormParameter("k", settings.ApiKey)
                          .AddFormParameter("d", string.Join("|", settings.DeviceIds))
                          .AddFormParameter("t", title)
                          .AddFormParameter("m", message)
                          .AddFormParameter("pr", settings.Priority);

            if ((PushsaferPriority)settings.Priority == PushsaferPriority.Emergency)
            {
                requestBuilder.AddFormParameter("re", settings.Retry);
                requestBuilder.AddFormParameter("ex", settings.Expire);
            }

            if (!settings.Sound.IsNullOrWhiteSpace())
            {
                requestBuilder.AddFormParameter("s", settings.Sound);
            }

            if (!settings.Vibration.IsNullOrWhiteSpace())
            {
                requestBuilder.AddFormParameter("v", settings.Vibration);
            }

            if (!settings.Icon.IsNullOrWhiteSpace())
            {
                requestBuilder.AddFormParameter("i", settings.Icon);
            }

            if (!settings.IconColor.IsNullOrWhiteSpace())
            {
                requestBuilder.AddFormParameter("c", settings.IconColor);
            }

            var request = requestBuilder.Build();

            var response = _httpClient.Post(request);

            // https://www.pushsafer.com/en/pushapi#api-message
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpException(request, response);
            }
        }

        public ValidationFailure Test(PushsaferSettings settings)
        {
            try
            {
                const string title = "Test Notification";
                const string body = "This is a test message from Radarr";

                SendNotification(title, body, settings);
            }
            catch (HttpException ex)
            {
                _logger.Error(ex, "Unable to send test message");

                return (int)ex.Response.StatusCode switch
                {
                    250 => new ValidationFailure("ApiKey", "API key is invalid"),
                    270 => new ValidationFailure("DeviceIds", "Device ID is invalid"),
                    275 => new ValidationFailure("DeviceIds", "Device Group ID is invalid"),
                    _ => null,
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("ApiKey", "Unable to send test message");
            }

            return null;
        }
    }
}
