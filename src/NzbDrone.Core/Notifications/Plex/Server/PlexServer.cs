using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Notifications.Plex.PlexTv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Plex.Server
{
    public class PlexServer : NotificationBase<PlexServerSettings>
    {
        private readonly IPlexServerService _plexServerService;
        private readonly IPlexTvService _plexTvService;
        private readonly Logger _logger;

        private class PlexUpdateQueue
        {
            public Dictionary<int, Movie> Pending { get; } = new Dictionary<int, Movie>();
            public bool Refreshing { get; set; }
        }

        private readonly ICached<PlexUpdateQueue> _pendingMoviesCache;

        public PlexServer(IPlexServerService plexServerService, IPlexTvService plexTvService, ICacheManager cacheManager, Logger logger)
        {
            _plexServerService = plexServerService;
            _plexTvService = plexTvService;
            _logger = logger;

            _pendingMoviesCache = cacheManager.GetRollingCache<PlexUpdateQueue>(GetType(), "pendingSeries", TimeSpan.FromDays(1));
        }

        public override string Link => "https://www.plex.tv/";
        public override string Name => "Plex Media Server";

        public override void OnDownload(DownloadMessage message)
        {
            UpdateIfEnabled(message.Movie);
        }

        public override void OnMovieRename(Movie movie, List<RenamedMovieFile> renamedFiles)
        {
            UpdateIfEnabled(movie);
        }

        public override void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            UpdateIfEnabled(deleteMessage.Movie);
        }

        public override void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            if (deleteMessage.DeletedFiles)
            {
                UpdateIfEnabled(deleteMessage.Movie);
            }
        }

        private void UpdateIfEnabled(Movie movie)
        {
            if (Settings.UpdateLibrary)
            {
                _logger.Debug("Scheduling library update for movie {0} {1}", movie.Id, movie.Title);
                var queue = _pendingMoviesCache.Get(Settings.Host, () => new PlexUpdateQueue());
                lock (queue)
                {
                    queue.Pending[movie.Id] = movie;
                }
            }
        }

        public override void ProcessQueue()
        {
            PlexUpdateQueue queue = _pendingMoviesCache.Find(Settings.Host);
            if (queue == null)
            {
                return;
            }

            lock (queue)
            {
                if (queue.Refreshing)
                {
                    return;
                }

                queue.Refreshing = true;
            }

            try
            {
                while (true)
                {
                    List<Movie> refreshingMovies;
                    lock (queue)
                    {
                        if (queue.Pending.Empty())
                        {
                            queue.Refreshing = false;
                            return;
                        }

                        refreshingMovies = queue.Pending.Values.ToList();
                        queue.Pending.Clear();
                    }

                    if (Settings.UpdateLibrary)
                    {
                        _logger.Debug("Performing library update for {0} movies", refreshingMovies.Count);
                        _plexServerService.UpdateLibrary(refreshingMovies, Settings);
                    }
                }
            }
            catch
            {
                lock (queue)
                {
                    queue.Refreshing = false;
                }

                throw;
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_plexServerService.Test(Settings));

            return new ValidationResult(failures);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                Settings.Validate().Filter("ConsumerKey", "ConsumerSecret").ThrowOnError();

                return _plexTvService.GetPinUrl();
            }
            else if (action == "continueOAuth")
            {
                Settings.Validate().Filter("ConsumerKey", "ConsumerSecret").ThrowOnError();

                if (query["callbackUrl"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam callbackUrl invalid.");
                }

                if (query["id"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam id invalid.");
                }

                if (query["code"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam code invalid.");
                }

                return _plexTvService.GetSignInUrl(query["callbackUrl"], Convert.ToInt32(query["id"]), query["code"]);
            }
            else if (action == "getOAuthToken")
            {
                Settings.Validate().Filter("ConsumerKey", "ConsumerSecret").ThrowOnError();

                if (query["pinId"].IsNullOrWhiteSpace())
                {
                    throw new BadRequestException("QueryParam pinId invalid.");
                }

                var authToken = _plexTvService.GetAuthToken(Convert.ToInt32(query["pinId"]));

                return new
                {
                    authToken
                };
            }

            return new { };
        }
    }
}
