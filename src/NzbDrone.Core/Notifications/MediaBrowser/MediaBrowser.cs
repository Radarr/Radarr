using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Emby
{
    public class MediaBrowser : NotificationBase<MediaBrowserSettings>
    {
        private readonly IMediaBrowserService _mediaBrowserService;

        public MediaBrowser(IMediaBrowserService mediaBrowserService)
        {
            _mediaBrowserService = mediaBrowserService;
        }

        public override string Link => "https://emby.media/";
        public override string Name => "Emby";

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
                _mediaBrowserService.UpdateMovies(Settings, message.Movie, "Created");
            }
        }

        public override void OnMovieRename(Movie movie, List<RenamedMovieFile> renamedFiles)
        {
            if (Settings.UpdateLibrary)
            {
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

        public override void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            if (deleteMessage.DeletedFiles)
            {
                if (Settings.Notify)
                {
                    _mediaBrowserService.Notify(Settings, MOVIE_DELETED_TITLE_BRANDED, deleteMessage.Message);
                }

                if (Settings.UpdateLibrary)
                {
                    _mediaBrowserService.UpdateMovies(Settings, deleteMessage.Movie, "Deleted");
                }
            }
        }

        public override void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            if (Settings.Notify)
            {
                _mediaBrowserService.Notify(Settings, MOVIE_FILE_DELETED_TITLE_BRANDED, deleteMessage.Message);
            }

            if (Settings.UpdateLibrary)
            {
                _mediaBrowserService.UpdateMovies(Settings, deleteMessage.Movie, "Deleted");
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
