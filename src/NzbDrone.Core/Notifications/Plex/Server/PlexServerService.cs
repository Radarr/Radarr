using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Plex.Server
{
    public interface IPlexServerService
    {
        void UpdateLibrary(Movie movie, PlexServerSettings settings);
        void UpdateLibrary(IEnumerable<Movie> movie, PlexServerSettings settings);
        ValidationFailure Test(PlexServerSettings settings);
    }

    public class PlexServerService : IPlexServerService
    {
        private readonly ICached<Version> _versionCache;
        private readonly IPlexServerProxy _plexServerProxy;
        private readonly IRootFolderService _rootFolderService;
        private readonly Logger _logger;

        public PlexServerService(ICacheManager cacheManager, IPlexServerProxy plexServerProxy, IRootFolderService rootFolderService, Logger logger)
        {
            _versionCache = cacheManager.GetCache<Version>(GetType(), "versionCache");
            _plexServerProxy = plexServerProxy;
            _rootFolderService = rootFolderService;
            _logger = logger;
        }

        public void UpdateLibrary(Movie movie, PlexServerSettings settings)
        {
            UpdateLibrary(new[] { movie }, settings);
        }

        public void UpdateLibrary(IEnumerable<Movie> multipleMovies, PlexServerSettings settings)
        {
            try
            {
                _logger.Debug("Sending Update Request to Plex Server");
                var watch = Stopwatch.StartNew();

                var version = _versionCache.Get(settings.Host, () => GetVersion(settings), TimeSpan.FromHours(2));
                ValidateVersion(version);

                var sections = GetSections(settings);

                foreach (var movie in multipleMovies)
                {
                    UpdateSections(movie, sections, settings);
                }

                _logger.Debug("Finished sending Update Request to Plex Server (took {0} ms)", watch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to Update Plex host: " + settings.Host);
                throw;
            }
        }

        private List<PlexSection> GetSections(PlexServerSettings settings)
        {
            _logger.Debug("Getting sections from Plex host: {0}", settings.Host);

            return _plexServerProxy.GetMovieSections(settings).ToList();
        }

        private void ValidateVersion(Version version)
        {
            if (version >= new Version(1, 3, 0) && version < new Version(1, 3, 1))
            {
                throw new PlexVersionException("Found version {0}, upgrade to PMS 1.3.1 to fix library updating and then restart Radarr", version);
            }
        }

        private Version GetVersion(PlexServerSettings settings)
        {
            _logger.Debug("Getting version from Plex host: {0}", settings.Host);

            var rawVersion = _plexServerProxy.Version(settings);
            var version = new Version(Regex.Match(rawVersion, @"^(\d+[.-]){4}").Value.Trim('.', '-'));

            return version;
        }

        private void UpdateSections(Movie movie, List<PlexSection> sections, PlexServerSettings settings)
        {
            var rootFolderPath = _rootFolderService.GetBestRootFolderPath(movie.Path);
            var movieRelativePath = rootFolderPath.GetRelativePath(movie.Path);

            // Try to update a matching section location before falling back to updating all section locations.
            foreach (var section in sections)
            {
                foreach (var location in section.Locations)
                {
                    var rootFolder = new OsPath(rootFolderPath);
                    var mappedPath = new OsPath(settings.MapTo) + (rootFolder - new OsPath(settings.MapFrom));

                    if (location.Path.PathEquals(mappedPath.FullPath))
                    {
                        _logger.Debug("Updating matching section location, {0}", location.Path);
                        UpdateSectionPath(movieRelativePath, section, location, settings);

                        return;
                    }
                }
            }

            _logger.Debug("Unable to find matching section location, updating all Movie sections");

            foreach (var section in sections)
            {
                foreach (var location in section.Locations)
                {
                    UpdateSectionPath(movieRelativePath, section, location, settings);
                }
            }
        }

        private void UpdateSectionPath(string movieRelativePath, PlexSection section, PlexSectionLocation location, PlexServerSettings settings)
        {
            var separator = location.Path.Contains('\\') ? "\\" : "/";
            var locationRelativePath = movieRelativePath.Replace("\\", separator).Replace("/", separator);

            // Plex location paths trim trailing extraneous separator characters, so it doesn't need to be trimmed
            var pathToUpdate = $"{location.Path}{separator}{locationRelativePath}";

            _logger.Debug("Updating section location, {0}", location.Path);
            _plexServerProxy.Update(section.Id, pathToUpdate, settings);
        }

        public ValidationFailure Test(PlexServerSettings settings)
        {
            try
            {
                _versionCache.Remove(settings.Host);
                var sections = GetSections(settings);

                if (sections.Empty())
                {
                    return new ValidationFailure("Host", "At least one Movie library is required");
                }
            }
            catch (PlexAuthenticationException ex)
            {
                _logger.Error(ex, "Unable to connect to Plex Media Server");
                return new ValidationFailure("AuthToken", "Invalid authentication token");
            }
            catch (PlexException ex)
            {
                return new NzbDroneValidationFailure("Host", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to connect to Plex Media Server");

                return new NzbDroneValidationFailure("Host", "Unable to connect to Plex Media Server")
                       {
                           DetailedDescription = ex.Message
                       };
            }

            return null;
        }
    }
}
