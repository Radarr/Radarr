using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Emby
{
    public class MediaBrowser : NotificationBase<MediaBrowserSettings>
    {
        private readonly IMediaBrowserService _mediaBrowserService;
        private readonly Logger _logger;

        public MediaBrowser(IMediaBrowserService mediaBrowserService, Logger logger)
        {
            _mediaBrowserService = mediaBrowserService;
            _logger = logger;
        }

        public override string Link => "https://emby.media/";
        public override string Name => "Jellyfin/Emby";

        public override void OnGrab(GrabMessage grabMessage)
        {
            if (Settings.Notify)
            {
                _mediaBrowserService.Notify(Settings, MOVIE_GRABBED_TITLE_BRANDED, grabMessage.Message);
            }
        }

        public override void OnDownload(DownloadMessage message)
        {
            if (Settings.Notify)
            {
                _mediaBrowserService.Notify(Settings, MOVIE_DOWNLOADED_TITLE_BRANDED, message.Message);
            }

            if (Settings.UpdateLibrary)
            {
                if (Settings.UpdateLibraryDelay > 0)
                {
                    var timeSpan = new TimeSpan(0, Settings.UpdateLibraryDelay, 0);
                    System.Threading.Thread.Sleep(timeSpan);
                }

                _logger.Debug("{0} - Scheduling library update for created movie {1} {2}", Name, message.Movie.Id, message.Movie.Title);

                _mediaBrowserService.UpdateMovies(Settings, message.Movie, "Created");
            }
        }

        public override void OnMovieRename(Movie movie)
        {
            if (Settings.UpdateLibrary)
            {
                if (Settings.UpdateLibraryDelay > 0)
                {
                    var timeSpan = new TimeSpan(0, Settings.UpdateLibraryDelay, 0);
                    System.Threading.Thread.Sleep(timeSpan);
                }

                _logger.Debug("{0} - Scheduling library update for modified movie {1} {2}", Name, movie.Id, movie.Title);

                _mediaBrowserService.UpdateMovies(Settings, movie, "Modified");
            }
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck message)
        {
            if (Settings.Notify)
            {
                _mediaBrowserService.Notify(Settings, HEALTH_ISSUE_TITLE_BRANDED, message.Message);
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_mediaBrowserService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
