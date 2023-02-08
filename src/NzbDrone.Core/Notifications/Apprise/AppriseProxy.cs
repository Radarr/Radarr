using System;

using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Notifications.Apprise
{
    public interface IAppriseProxy
    {
        void SendNotification(string title, string message, AppriseSettings settings);

        ValidationFailure Test(AppriseSettings settings);
    }

    public class AppriseProxy : IAppriseProxy
    {
        private const string DEFAULT_PUSH_URL = "http://localhost:8000/notify/apprise";

        private readonly IHttpClient _httpClient;

        private readonly Logger _logger;

        public AppriseProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void SendNotification(string title, string message, AppriseSettings settings)
        {
            var error = false;

            var serverUrl = settings.ServerUrl.IsNullOrWhiteSpace() ? AppriseProxy.DEFAULT_PUSH_URL : settings.ServerUrl;

            var request = BuildTopicRequest(serverUrl, settings);
            var payload = CreatePayload(title, message, settings);

            try
            {
                SendNotification(payload, request);
            }
            catch (AppriseException ex)
            {
                _logger.Error(ex, "Unable to send test message to server: {0}", serverUrl);
                error = true;
            }

            if (error)
            {
                throw new AppriseException("Unable to send apprise notifications");
            }
        }

        private HttpRequestBuilder BuildTopicRequest(string serverUrl, AppriseSettings settings)
        {
            var trimServerUrl = serverUrl.TrimEnd('/');

            var requestBuilder = new HttpRequestBuilder($"{trimServerUrl}").Post();
            requestBuilder.Accept(HttpAccept.Json);

            requestBuilder.Headers.Add("Content-Type", "application/json");

            if (settings.Tag.IsNotNullOrWhiteSpace())
            {
                requestBuilder.AddQueryParam("tag", settings.Tag);
            }

            return requestBuilder;
        }

        public ValidationFailure Test(AppriseSettings settings)
        {
            try
            {
                const string title = "Radarr - Test Notification";

                const string body = "This is a test message from Radarr";

                SendNotification(title, body, settings);
            }
            catch (HttpException ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("ServerUrl", "Unable to send test message");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("", "Unable to send test message");
            }

            return null;
        }

        private void SendNotification(ApprisePayload payload, HttpRequestBuilder requestBuilder)
        {
            try
            {
                var request = requestBuilder.Build();
                request.SetContent(payload.ToJson());

                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                throw new AppriseException("Unable to send text message: {0}. HttpStatus: {1}", ex, ex.Response.StatusCode, ex.Message);
            }
        }

        private static ApprisePayload CreatePayload(string title, string message, AppriseSettings settings)
        {
            var payload = new ApprisePayload
            {
                Title = title,
                Body = message,
                NotificationType = (ApprisePriority)settings.NotificationType
            };

            if (settings.Tag.IsNotNullOrWhiteSpace())
            {
                payload.Tag = settings.Tag;
            }

            return payload;
        }
    }
}
