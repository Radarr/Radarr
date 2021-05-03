using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Notifications.Notifiarr
{
    public interface INotifiarrProxy
    {
        void SendNotification(StringDictionary message, NotifiarrSettings settings);
        ValidationFailure Test(NotifiarrSettings settings);
    }

    public class NotifiarrProxy : INotifiarrProxy
    {
        private const string URL = "https://notifiarr.com/notifier.php";
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public NotifiarrProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void SendNotification(StringDictionary message, NotifiarrSettings settings)
        {
            try
            {
                ProcessNotification(message, settings);
            }
            catch (NotifiarrException ex)
            {
                _logger.Error(ex, "Unable to send notification");
                throw new NotifiarrException("Unable to send notification");
            }
        }

        public ValidationFailure Test(NotifiarrSettings settings)
        {
            try
            {
                var variables = new StringDictionary();
                variables.Add("Radarr_EventType", "Test");

                SendNotification(variables, settings);
                return null;
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "API key is invalid: " + ex.Message);
                    return new ValidationFailure("APIKey", "API key is invalid");
                }

                _logger.Error(ex, "Unable to send test message: " + ex.Message);
                return new ValidationFailure("APIKey", "Unable to send test notification");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test notification: " + ex.Message);
                return new ValidationFailure("", "Unable to send test notification");
            }
        }

        private void ProcessNotification(StringDictionary message, NotifiarrSettings settings)
        {
            try
            {
                var requestBuilder = new HttpRequestBuilder(URL).Post();
                requestBuilder.AddFormParameter("api", settings.APIKey).Build();

                foreach (string key in message.Keys)
                {
                    requestBuilder.AddFormParameter(key, message[key]);
                }

                var request = requestBuilder.Build();

                _httpClient.Post(request);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    _logger.Error(ex, "API key is invalid");
                    throw;
                }

                throw new NotifiarrException("Unable to send notification", ex);
            }
        }
    }
}
