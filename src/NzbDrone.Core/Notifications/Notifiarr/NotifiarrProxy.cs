using System;
using System.Collections.Specialized;
using FluentValidation.Results;
using NLog;
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
                switch ((int)ex.Response.StatusCode)
                {
                    case 401:
                        _logger.Error(ex, "API key is invalid: " + ex.Message);
                        return new ValidationFailure("APIKey", "API key is invalid");
                    case 400:
                    case 520:
                    case 521:
                    case 522:
                    case 523:
                    case 524:
                        _logger.Error(ex, "Unable to send test notification: " + ex.Message);
                        return new ValidationFailure("", "Unable to send test notification");
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
                var url = settings.Environment == (int)NotifiarrEnvironment.Development ? "https://dev.notifiarr.com" : "https://notifiarr.com";
                var requestBuilder = new HttpRequestBuilder(url + "/notifier.php").Post();
                requestBuilder.AddFormParameter("api", settings.APIKey).Build();
                requestBuilder.AddFormParameter("instanceName", settings.InstanceName).Build();

                foreach (string key in message.Keys)
                {
                    requestBuilder.AddFormParameter(key, message[key]);
                }

                var request = requestBuilder.Build();

                _httpClient.Post(request);
            }
            catch (HttpException ex)
            {
                switch ((int)ex.Response.StatusCode)
                {
                    case 401:
                        _logger.Error(ex, "API key is invalid");
                        throw;
                    case 400:
                    case 520:
                    case 521:
                    case 522:
                    case 523:
                    case 524:
                        _logger.Error(ex, "Unable to send notification");
                        throw;
                }

                throw new NotifiarrException("Unable to send notification", ex);
            }
        }
    }
}
