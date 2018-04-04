using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexClient : NotificationBase<PlexClientSettings>
    {
        private readonly IPlexClientService _plexClientService;

        public override string Link => "https://www.plex.tv/";
        public override string Name => "Plex Media Center";

        public PlexClient(IPlexClientService plexClientService)
        {
            _plexClientService = plexClientService;
        }

        public override void OnGrab(GrabMessage grabMessage)
        {
            _plexClientService.Notify(Settings, ALBUM_GRABBED_TITLE_BRANDED, grabMessage.Message);
        }

        public override void OnDownload(TrackDownloadMessage message)
        {
            _plexClientService.Notify(Settings, TRACK_DOWNLOADED_TITLE_BRANDED, message.Message);
        }


        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_plexClientService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
