using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexServer : NotificationBase<PlexServerSettings>
    {
        private readonly IPlexServerService _plexServerService;

        public PlexServer(IPlexServerService plexServerService)
        {
            _plexServerService = plexServerService;
        }

        public override string Link => "http://www.plexapp.com/";

        public override void OnGrab(GrabMessage grabMessage)
        {
        }

        public override void OnDownload(DownloadMessage message)
        {
            UpdateIfEnabled(message.Movie);
        }

        public override void OnMovieRename(Movie movie)
        {
            UpdateIfEnabled(movie);
        }
		
        private void UpdateIfEnabled(Movie movie)
        {
            if (Settings.UpdateLibrary)
            {
                _plexServerService.UpdateMovieSections(movie, Settings);
            }
        }

        public override string Name => "Plex Media Server";

        public override bool SupportsOnGrab => false;

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_plexServerService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
