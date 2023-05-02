using System;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public interface IXbmcService
    {
        void Notify(XbmcSettings settings, string title, string message);
        void UpdateMovie(XbmcSettings settings, Movie movie);
        void Clean(XbmcSettings settings);
        ValidationFailure Test(XbmcSettings settings, string message);
    }

    public class XbmcService : IXbmcService
    {
        private readonly IXbmcJsonApiProxy _proxy;
        private readonly Logger _logger;

        public XbmcService(IXbmcJsonApiProxy proxy,
                           Logger logger)
        {
            _proxy = proxy;
            _logger = logger;
        }

        public void Notify(XbmcSettings settings, string title, string message)
        {
            _proxy.Notify(settings, title, message);
        }

        public void UpdateMovie(XbmcSettings settings, Movie movie)
        {
            if (CheckIfVideoPlayerOpen(settings))
            {
                _logger.Debug("Video is currently playing, skipping library update");

                return;
            }

            UpdateLibrary(settings, movie);
        }

        public void Clean(XbmcSettings settings)
        {
            if (CheckIfVideoPlayerOpen(settings))
            {
                _logger.Debug("Video is currently playing, skipping library clean");

                return;
            }

            _proxy.CleanLibrary(settings);
        }

        public string GetMoviePath(XbmcSettings settings, Movie movie)
        {
            var allMovies = _proxy.GetMovies(settings);

            if (!allMovies.Any())
            {
                _logger.Debug("No Movies returned from Kodi");
                return null;
            }

            var matchingMovies = allMovies.FirstOrDefault(s =>
            {
                return s.ImdbNumber == movie.ImdbId || s.Label == movie.Title;
            });

            if (matchingMovies != null)
            {
                return matchingMovies.File;
            }

            return null;
        }

        private void UpdateLibrary(XbmcSettings settings, Movie movie)
        {
            try
            {
                var moviePath = GetMoviePath(settings, movie);

                if (moviePath != null)
                {
                    _logger.Debug("Updating movie {0} (Path: {1}) on Kodi host: {2}", movie, moviePath, settings.Address);
                }
                else
                {
                    _logger.Debug("Movie {0} doesn't exist on Kodi host: {1}, Updating Entire Library", movie, settings.Address);
                }

                var response = _proxy.UpdateLibrary(settings, moviePath);

                if (!response.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.Debug("Failed to update library for: {0}", settings.Address);
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, ex.Message);
            }
        }

        private bool CheckIfVideoPlayerOpen(XbmcSettings settings)
        {
            if (settings.AlwaysUpdate)
            {
                return false;
            }

            _logger.Debug("Determining if there are any active players on Kodi host: {0}", settings.Address);
            var activePlayers = _proxy.GetActivePlayers(settings);

            return activePlayers.Any(a => a.Type.Equals("video"));
        }

        public ValidationFailure Test(XbmcSettings settings, string message)
        {
            try
            {
                Notify(settings, "Test Notification", message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("Host", "Unable to send test message");
            }

            return null;
        }
    }
}
