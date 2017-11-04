﻿using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.MediaBrowser
{
    public class MediaBrowser : NotificationBase<MediaBrowserSettings>
    {
        private readonly IMediaBrowserService _mediaBrowserService;

        public MediaBrowser(IMediaBrowserService mediaBrowserService)
        {
            _mediaBrowserService = mediaBrowserService;
        }

        public override string Link => "http://mediabrowser.tv/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            const string title = "Radarr - Movie Grabbed";

            if (Settings.Notify)
            {
                _mediaBrowserService.Notify(Settings, title, grabMessage.Message);
            }
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string title = "Radarr - Movie Downloaded";

            if (Settings.Notify)
            {
                _mediaBrowserService.Notify(Settings, title, message.Message);
            }

            if (Settings.UpdateLibrary)
            {
                _mediaBrowserService.UpdateMovies(Settings, message.Movie);
            }
        }

        public override void OnMovieRename(Movie movie)
        {
            if (Settings.UpdateLibrary)
            {
                _mediaBrowserService.UpdateMovies(Settings, movie);
            }
        }

        public override string Name => "Emby (Media Browser)";

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_mediaBrowserService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
