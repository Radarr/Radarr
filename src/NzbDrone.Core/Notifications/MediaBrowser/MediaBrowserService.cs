using System;
using System.Collections.Generic;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Emby
{
    public interface IMediaBrowserService
    {
        void Notify(MediaBrowserSettings settings, string title, string message);
        void UpdateMovies(MediaBrowserSettings settings, Movie movie, string updateType);
        ValidationFailure Test(MediaBrowserSettings settings);
    }

    public class MediaBrowserService : IMediaBrowserService
    {
        private readonly MediaBrowserProxy _proxy;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public MediaBrowserService(MediaBrowserProxy proxy, ILocalizationService localizationService, Logger logger)
        {
            _proxy = proxy;
            _localizationService = localizationService;
            _logger = logger;
        }

        public void Notify(MediaBrowserSettings settings, string title, string message)
        {
            _proxy.Notify(settings, title, message);
        }

        public void UpdateMovies(MediaBrowserSettings settings, Movie movie, string updateType)
        {
            _proxy.UpdateMovies(settings, movie.Path, updateType);
        }

        public ValidationFailure Test(MediaBrowserSettings settings)
        {
            try
            {
                _logger.Debug("Testing connection to Emby/Jellyfin : {0}", settings.Address);

                Notify(settings, "Test from Radarr", "Success! MediaBrowser has been successfully configured!");
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new ValidationFailure("ApiKey", _localizationService.GetLocalizedString("NotificationsValidationInvalidApiKey"));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("Host", _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }
    }
}
