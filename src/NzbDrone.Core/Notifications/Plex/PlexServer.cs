using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexServer : NotificationBase<PlexServerSettings>
    {
        private readonly IPlexServerService _plexServerService;

        public PlexServer(IPlexServerService plexServerService)
        {
            _plexServerService = plexServerService;
        }

        public override string Link => "https://www.plex.tv/";
        public override string Name => "Plex Media Server";

        public override void OnDownload(TrackDownloadMessage message)
        {
            UpdateIfEnabled(message.Artist);
        }

        public override void OnAlbumDownload(AlbumDownloadMessage message)
        {
            UpdateIfEnabled(message.Artist);
        }

        public override void OnRename(Artist artist)
        {
            UpdateIfEnabled(artist);
        }

        private void UpdateIfEnabled(Artist artist)
        {
            if (Settings.UpdateLibrary)
            {
                _plexServerService.UpdateLibrary(artist, Settings);
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_plexServerService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
