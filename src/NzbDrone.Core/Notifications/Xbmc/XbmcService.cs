using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications.Xbmc.Model;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public interface IXbmcService
    {
        void Notify(XbmcSettings settings, string title, string message);
        void Update(XbmcSettings settings, Movie movie);
        void Clean(XbmcSettings settings);
        ValidationFailure Test(XbmcSettings settings, string message);
    }

    public class XbmcService : IXbmcService
    {
        private static readonly char[] PathSeparators = { '/', '\\' };
        private static readonly HashSet<string> DiscFolders = new(StringComparer.OrdinalIgnoreCase) { "BDMV", "VIDEO_TS" };

        private readonly IXbmcJsonApiProxy _proxy;
        private readonly ICached<Version> _versionCache;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public XbmcService(IXbmcJsonApiProxy proxy, ICacheManager cacheManager, ILocalizationService localizationService, Logger logger)
        {
            _proxy = proxy;
            _localizationService = localizationService;
            _logger = logger;

            _versionCache = cacheManager.GetCache<Version>(GetType(), "versionCache");
        }

        internal int ScanWaitAttempts { get; set; } = 30;
        internal TimeSpan ScanWaitInterval { get; set; } = TimeSpan.FromMilliseconds(500);

        public void Notify(XbmcSettings settings, string title, string message)
        {
            _proxy.Notify(settings, title, message);
        }

        public void Update(XbmcSettings settings, Movie movie)
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

            if (!WaitUntilIdle(settings, "clean"))
            {
                return;
            }

            _proxy.CleanLibrary(settings);
        }

        public string GetMovieFolder(XbmcSettings settings, Movie movie)
        {
            var matchingFiles = _proxy.GetMovies(settings, SupportsUniqueIds(settings))
                .Where(m => IsMatchingMovie(m, movie))
                .Select(m => m.File)
                .Distinct()
                .ToList();

            if (matchingFiles.Empty())
            {
                _logger.Debug("Movie {0} doesn't exist on Kodi host: {1}, Updating Entire Library", movie, settings.Address);

                return null;
            }

            // Another Radarr instance or an old folder can hold a copy of the same movie, so only a folder that ends like the movie's path qualifies
            var folders = matchingFiles.Select(GetFolder)
                .Where(f => f != null && IsSameFolder(f, movie.Path))
                .Distinct()
                .ToList();

            if (folders.Count != 1)
            {
                _logger.Debug("Unable to find the folder of movie {0} (Kodi paths: {1}) on Kodi host: {2}, Updating Entire Library", movie, string.Join(", ", matchingFiles), settings.Address);

                return null;
            }

            _logger.Debug("Updating movie {0} (Kodi path: {1}) on Kodi host: {2}", movie, folders[0], settings.Address);

            return folders[0];
        }

        // Kodi 17 (JSON-RPC 8) added the unique IDs, older versions reject a request for them
        private bool SupportsUniqueIds(XbmcSettings settings)
        {
            var version = _versionCache.Get(settings.Address, () => _proxy.GetJsonVersion(settings), TimeSpan.FromHours(2));

            return version.Major >= 8;
        }

        private static bool IsMatchingMovie(XbmcMovie xbmcMovie, Movie movie)
        {
            // imdbnumber holds the scraper's default ID, which is the TMDb ID with the TMDb scraper, so match on the unique IDs first.
            // The TMDb ID decides when Kodi has one, so a stale IMDb ID can't match another movie.
            if (xbmcMovie.UniqueId != null)
            {
                if (movie.TmdbId > 0 && xbmcMovie.UniqueId.TryGetValue("tmdb", out var kodiTmdbId) && int.TryParse(kodiTmdbId, out var tmdbId))
                {
                    return tmdbId == movie.TmdbId;
                }

                if (movie.ImdbId.IsNotNullOrWhiteSpace() && xbmcMovie.UniqueId.TryGetValue("imdb", out var imdbId))
                {
                    return imdbId == movie.ImdbId;
                }
            }

            return movie.ImdbId.IsNotNullOrWhiteSpace() && xbmcMovie.ImdbNumber == movie.ImdbId;
        }

        // Kodi skips a scan when the path is not a folder, so scan the folder that holds the movie's file
        private static string GetFolder(string moviePath)
        {
            if (moviePath.IsNullOrWhiteSpace())
            {
                return null;
            }

            // A multi-part movie is "stack://<part 1> , <part 2>" with all parts in the same folder, and the folder of the first part is not escaped
            if (moviePath.StartsWith("stack://", StringComparison.OrdinalIgnoreCase))
            {
                moviePath = moviePath.Substring("stack://".Length).Split(" , ")[0];
            }

            var folder = GetParentDirectory(moviePath);

            // Disc structures are stored as the BDMV or VIDEO_TS folder inside the movie folder
            if (folder != null && DiscFolders.Contains(GetLastSegments(folder, 1).FirstOrDefault()))
            {
                folder = GetParentDirectory(folder.TrimEnd(PathSeparators));
            }

            return folder;
        }

        // Kodi and Radarr can reach the same folder through different roots, so compare the movie folder and its parent by name
        private static bool IsSameFolder(string kodiFolder, string moviePath)
        {
            if (moviePath.IsNullOrWhiteSpace())
            {
                return false;
            }

            var kodiSegments = GetLastSegments(kodiFolder, 2);
            var movieSegments = GetLastSegments(moviePath, 2);

            return kodiSegments.Length == 2 && kodiSegments.SequenceEqual(movieSegments, StringComparer.OrdinalIgnoreCase);
        }

        private static string GetParentDirectory(string path)
        {
            var separatorIndex = path.LastIndexOfAny(PathSeparators);

            return separatorIndex > 0 ? path.Substring(0, separatorIndex + 1) : null;
        }

        private static string[] GetLastSegments(string path, int count)
        {
            var segments = path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);

            return segments.Skip(Math.Max(0, segments.Length - count)).ToArray();
        }

        private void UpdateLibrary(XbmcSettings settings, Movie movie)
        {
            try
            {
                var movieFolder = GetMovieFolder(settings, movie);

                if (!WaitUntilIdle(settings, "update"))
                {
                    return;
                }

                var response = _proxy.UpdateLibrary(settings, movieFolder);

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

        // Kodi stops a running scan instead of starting a new one, and refuses to clean while a scan or clean is running
        private bool WaitUntilIdle(XbmcSettings settings, string libraryTask)
        {
            if (!WaitForScanToFinish(settings, out var waited))
            {
                _logger.Debug("Kodi is still scanning, skipping library {0} on Kodi host: {1}", libraryTask, settings.Address);

                return false;
            }

            // A video can start playing while waiting
            if (waited && CheckIfVideoPlayerOpen(settings))
            {
                _logger.Debug("Video is currently playing, skipping library {0}", libraryTask);

                return false;
            }

            return true;
        }

        private bool WaitForScanToFinish(XbmcSettings settings, out bool waited)
        {
            waited = false;

            for (var attempt = 0; attempt < ScanWaitAttempts; attempt++)
            {
                try
                {
                    if (!_proxy.IsScanningVideoLibrary(settings))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    // The check only prevents a request that Kodi would ignore, so carry on as if Kodi were idle
                    _logger.Debug(ex, "Unable to check whether Kodi is scanning on Kodi host: {0}", settings.Address);

                    return true;
                }

                waited = true;
                Thread.Sleep(ScanWaitInterval);
            }

            return false;
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
            _versionCache.Remove(settings.Address);

            try
            {
                Notify(settings, "Test Notification", message);
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
