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
            if (!settings.AlwaysUpdate)
            {
                _logger.Debug("Determining if there are any active players on XBMC host: {0}", settings.Address);
                var activePlayers = _proxy.GetActivePlayers(settings);

                if (activePlayers.Any(a => a.Type.Equals("video")))
                {
                    _logger.Debug("Video is currently playing, skipping library update");
                    return;
                }
            }

            UpdateMovieLibrary(settings, movie);
        }

        public void Clean(XbmcSettings settings)
        {
            _proxy.CleanLibrary(settings);
        }

        public string GetMoviePath(XbmcSettings settings, Movie movie)
        {
            var allMovies = _proxy.GetMovies(settings);

            if (!allMovies.Any())
            {
                _logger.Debug("No Movies returned from XBMC");
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

        private void UpdateMovieLibrary(XbmcSettings settings, Movie movie)
        {
            try
            {
                var moviePath = GetMoviePath(settings, movie);

                if (moviePath != null)
                {
                    moviePath = new OsPath(moviePath).Directory.FullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    _logger.Debug("Updating movie {0} (Path: {1}) on XBMC host: {2}", movie, moviePath, settings.Address);
                }
                else
                {
                    _logger.Debug("Movie {0} doesn't exist on XBMC host: {1}, Updating Entire Library", movie, settings.Address);
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
