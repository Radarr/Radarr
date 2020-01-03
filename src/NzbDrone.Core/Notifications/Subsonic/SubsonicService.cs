using System;
using FluentValidation.Results;
using NLog;

namespace NzbDrone.Core.Notifications.Subsonic
{
    public interface ISubsonicService
    {
        void Notify(SubsonicSettings settings, string message);
        void Update(SubsonicSettings settings);
        ValidationFailure Test(SubsonicSettings settings, string message);
    }

    public class SubsonicService : ISubsonicService
    {
        private readonly ISubsonicServerProxy _proxy;
        private readonly Logger _logger;

        public SubsonicService(ISubsonicServerProxy proxy,
                           Logger logger)
        {
            _proxy = proxy;
            _logger = logger;
        }

        public void Notify(SubsonicSettings settings, string message)
        {
            _proxy.Notify(settings, message);
        }

        public void Update(SubsonicSettings settings)
        {
            _proxy.Update(settings);
        }

        private string GetVersion(SubsonicSettings settings)
        {
            var result = _proxy.Version(settings);

            return result;
        }

        public ValidationFailure Test(SubsonicSettings settings, string message)
        {
            try
            {
                _logger.Debug("Determining version of Host: {0}", _proxy.GetBaseUrl(settings));
                var version = GetVersion(settings);
                _logger.Debug("Version is: {0}", version);
            }
            catch (SubsonicAuthenticationException ex)
            {
                _logger.Error(ex, "Unable to connect to Subsonic Server");
                return new ValidationFailure("Username", "Incorrect username or password");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to connect to Subsonic Server");
                return new ValidationFailure("Host", "Unable to connect to Subsonic Server");
            }

            return null;
        }
    }
}
