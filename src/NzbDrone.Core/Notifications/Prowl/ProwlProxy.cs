using System;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Rest;
using RestSharp;

namespace NzbDrone.Core.Notifications.Prowl
{
    public interface IProwlProxy
    {
        void SendNotification(string title, string message, string apiKey, ProwlPriority priority = ProwlPriority.Normal, string url = null);
        ValidationFailure Test(ProwlSettings settings);
    }

    public class ProwlProxy : IProwlProxy
    {
        private const string PUSH_URL = "https://api.prowlapp.com/publicapi/add";
        private readonly IRestClientFactory _restClientFactory;
        private readonly Logger _logger;

        public ProwlProxy(IRestClientFactory restClientFactory, Logger logger)
        {
            _restClientFactory = restClientFactory;
            _logger = logger;
        }

        public void SendNotification(string title, string message, string apiKey, ProwlPriority priority = ProwlPriority.Normal, string url = null)
        {
            try
            {
                var client = _restClientFactory.BuildClient(PUSH_URL);
                var request = new RestRequest(Method.POST);

                request.AddParameter("apikey", apiKey);
                request.AddParameter("application", BuildInfo.AppName);
                request.AddParameter("event", title);
                request.AddParameter("description", message);
                request.AddParameter("priority", priority);
                request.AddParameter("url", url);

                client.ExecuteAndValidate(request);
            }
            catch (RestException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "Apikey is invalid: {0}", apiKey);
                    throw new ProwlException("Apikey is invalid", ex);
                }

                throw new ProwlException("Unable to send text message: " + ex.Message, ex);
            }
        }

        public ValidationFailure Test(ProwlSettings settings)
        {
            try
            {
                const string title = "Test Notification";
                const string body = "This is a test message from Radarr";

                SendNotification(title, body, settings.ApiKey);
            }
            catch (Exception ex)
            {
                return new ValidationFailure("ApiKey", ex.Message);
            }

            return null;
        }
    }
}
