using System;
using FluentValidation.Results;
using NLog;
using RestSharp;
using NzbDrone.Core.Rest;

namespace NzbDrone.Core.Notifications.Gotify
{
    public interface IGotifyProxy
    {
        void SendNotification(string title, string message, GotifySettings settings);
        ValidationFailure Test(GotifySettings settings);
    }

    public class GotifyProxy : IGotifyProxy
    {
        private readonly Logger _logger;

        public GotifyProxy(Logger logger)
        {
            _logger = logger;
        }

        public void SendNotification(string title, string message, GotifySettings settings)
        {
            var url = $"{settings.GotifyServer}/message?token={settings.AppToken}";
            var client = RestClientFactory.BuildClient(url);
            var request = new RestRequest(Method.POST);
            request.AddParameter("title", title);
            request.AddParameter("message", message);
            request.AddParameter("priority", settings.Priority);

            client.ExecuteAndValidate(request);
        }

        public ValidationFailure Test(GotifySettings settings)
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
                return new ValidationFailure("", "Unable to send test message");
            }

            return null;
        }
    }
}
