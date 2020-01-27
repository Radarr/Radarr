using System;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Rest;
using RestSharp;

namespace NzbDrone.Core.Notifications.Join
{
    public interface IJoinProxy
    {
        void SendNotification(string title, string message, JoinSettings settings);
        ValidationFailure Test(JoinSettings settings);
    }

    public class JoinProxy : IJoinProxy
    {
        private const string URL = "https://joinjoaomgcd.appspot.com/_ah/api/messaging/v1/sendPush?";
        private readonly Logger _logger;

        public JoinProxy(Logger logger)
        {
            _logger = logger;
        }

        public void SendNotification(string title, string message, JoinSettings settings)
        {
            var request = new RestRequest(Method.GET);

            try
            {
                SendNotification(title, message, request, settings);
            }
            catch (JoinException ex)
            {
                _logger.Error(ex, "Unable to send Join message.");
                throw ex;
            }
        }

        public ValidationFailure Test(JoinSettings settings)
        {
            const string title = "Test Notification";
            const string body = "This is a test message from Radarr.";

            try
            {
                SendNotification(title, body, settings);
                return null;
            }
            catch (JoinInvalidDeviceException ex)
            {
                _logger.Error(ex, "Unable to send test Join message. Invalid Device IDs supplied.");
                return new ValidationFailure("DeviceIds", "Device IDs appear invalid.");
            }
            catch (JoinException ex)
            {
                _logger.Error(ex, "Unable to send test Join message.");
                return new ValidationFailure("ApiKey", ex.Message);
            }
            catch (RestException ex)
            {
                _logger.Error(ex, "Unable to send test Join message. Server connection failed.");
                return new ValidationFailure("ApiKey", "Unable to connect to Join API. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test Join message. Unknown error.");
                return new ValidationFailure("ApiKey", ex.Message);
            }
        }

        private void SendNotification(string title, string message, RestRequest request, JoinSettings settings)
        {
            var client = RestClientFactory.BuildClient(URL);

            if (settings.DeviceNames.IsNotNullOrWhiteSpace())
            {
                request.AddParameter("deviceNames", settings.DeviceNames);
            }
            else if (settings.DeviceIds.IsNotNullOrWhiteSpace())
            {
                request.AddParameter("deviceIds", settings.DeviceIds);
            }
            else
            {
                request.AddParameter("deviceId", "group.all");
            }

            request.AddParameter("apikey", settings.ApiKey);
            request.AddParameter("title", title);
            request.AddParameter("text", message);
            request.AddParameter("icon", "https://cdn.rawgit.com/Radarr/Radarr/develop/Logo/256.png"); // Use the Radarr logo.
            request.AddParameter("smallicon", "https://cdn.rawgit.com/Radarr/Radarr/develop/Logo/96-Outline-White.png"); // 96x96px with outline at 88x88px on a transparent background.
            request.AddParameter("priority", settings.Priority);

            var response = client.ExecuteAndValidate(request);
            var res = Json.Deserialize<JoinResponseModel>(response.Content);

            if (res.success)
            {
                return;
            }

            if (res.userAuthError)
            {
                throw new JoinAuthException("Authentication failed.");
            }

            if (res.errorMessage != null)
            {
                // Unfortunately hard coding this string here is the only way to determine that there aren't any devices to send to.
                // There isn't an enum or flag contained in the response that can be used instead.
                if (res.errorMessage.Equals("No devices to send to"))
                {
                    throw new JoinInvalidDeviceException(res.errorMessage);
                }

                // Oddly enough, rather than give us an "Invalid API key", the Join API seems to assume the key is valid,
                // but fails when doing a device lookup associated with that key.
                // In our case we are using "deviceIds" rather than "deviceId" so when the singular form error shows up
                // we know the API key was the fault.
                else if (res.errorMessage.Equals("No device to send message to"))
                {
                    throw new JoinAuthException("Authentication failed.");
                }

                throw new JoinException(res.errorMessage);
            }

            throw new JoinException("Unknown error. Join message failed to send.");
        }
    }
}
