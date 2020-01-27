﻿using System;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Rest;
using RestSharp;

namespace NzbDrone.Core.Notifications.Pushover
{
    public interface IPushoverProxy
    {
        void SendNotification(string title, string message, PushoverSettings settings);
        ValidationFailure Test(PushoverSettings settings);
    }

    public class PushoverProxy : IPushoverProxy
    {
        private const string URL = "https://api.pushover.net/1/messages.json";
        private readonly Logger _logger;

        public PushoverProxy(Logger logger)
        {
            _logger = logger;
        }

        public void SendNotification(string title, string message, PushoverSettings settings)
        {
            var client = RestClientFactory.BuildClient(URL);
            var request = new RestRequest(Method.POST);
            request.AddParameter("token", settings.ApiKey);
            request.AddParameter("user", settings.UserKey);
            request.AddParameter("title", title);
            request.AddParameter("message", message);
            request.AddParameter("priority", settings.Priority);

            if ((PushoverPriority)settings.Priority == PushoverPriority.Emergency)
            {
                request.AddParameter("retry", settings.Retry);
                request.AddParameter("expire", settings.Expire);
            }

            if (!settings.Sound.IsNullOrWhiteSpace())
            {
                request.AddParameter("sound", settings.Sound);
            }

            client.ExecuteAndValidate(request);
        }

        public ValidationFailure Test(PushoverSettings settings)
        {
            try
            {
                const string title = "Test Notification";
                const string body = "This is a test message from Radarr";

                SendNotification(title, body, settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message: " + ex.Message);
                return new ValidationFailure("ApiKey", "Unable to send test message");
            }

            return null;
        }
    }
}
