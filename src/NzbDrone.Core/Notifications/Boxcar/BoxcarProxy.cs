using System;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Rest;
using RestSharp;

namespace NzbDrone.Core.Notifications.Boxcar
{
    public interface IBoxcarProxy
    {
        void SendNotification(string title, string message, BoxcarSettings settings);
        ValidationFailure Test(BoxcarSettings settings);
    }

    public class BoxcarProxy : IBoxcarProxy
    {
        private const string URL = "https://new.boxcar.io/api/notifications";
        private readonly IRestClientFactory _restClientFactory;
        private readonly Logger _logger;

        public BoxcarProxy(IRestClientFactory restClientFactory, Logger logger)
        {
            _restClientFactory = restClientFactory;
            _logger = logger;
        }

        public void SendNotification(string title, string message, BoxcarSettings settings)
        {
            var request = new RestRequest(Method.POST);

            try
            {
                SendNotification(title, message, request, settings);
            }
            catch (BoxcarException ex)
            {
                _logger.Error(ex, "Unable to send message");
                throw new BoxcarException("Unable to send Boxcar notifications");
            }
        }

        public ValidationFailure Test(BoxcarSettings settings)
        {
            try
            {
                const string title = "Test Notification";
                const string body = "This is a test message from Readarr";

                SendNotification(title, body, settings);
                return null;
            }
            catch (RestException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "Access Token is invalid");
                    return new ValidationFailure("Token", "Access Token is invalid");
                }

                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("Token", "Unable to send test message");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("", "Unable to send test message");
            }
        }

        private void SendNotification(string title, string message, RestRequest request, BoxcarSettings settings)
        {
            try
            {
                var client = _restClientFactory.BuildClient(URL);

                request.AddParameter("user_credentials", settings.Token);
                request.AddParameter("notification[title]", title);
                request.AddParameter("notification[long_message]", message);
                request.AddParameter("notification[source_name]", BuildInfo.AppName);
                request.AddParameter("notification[icon_url]", "https://github.com/readarr/Readarr/raw/develop/Logo/64.png");

                client.ExecuteAndValidate(request);
            }
            catch (RestException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "Access Token is invalid");
                    throw;
                }

                throw new BoxcarException("Unable to send text message: " + ex.Message, ex);
            }
        }
    }
}
