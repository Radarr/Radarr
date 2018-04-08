using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Music;

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
        public override string Name => "Emby (Media Browser)";


        public override void OnGrab(GrabMessage grabMessage)
        {
            if (Settings.Notify)
            {
                _mediaBrowserService.Notify(Settings, ALBUM_GRABBED_TITLE_BRANDED, grabMessage.Message);
            }
        }

        public override void OnAlbumDownload(AlbumDownloadMessage message)
        {
            if (Settings.Notify)
            {
                _mediaBrowserService.Notify(Settings, ALBUM_DOWNLOADED_TITLE_BRANDED, message.Message);
            }

            if (Settings.UpdateLibrary)
            {
                _mediaBrowserService.Update(Settings, message.Artist);
            }
        }

        public override void OnDownload(TrackDownloadMessage message)
        {
            if (Settings.Notify)
            {
                _mediaBrowserService.Notify(Settings, TRACK_DOWNLOADED_TITLE_BRANDED, message.Message);
            }
        }

        public override void OnRename(Artist artist)
        {
            if (Settings.UpdateLibrary)
            {
                _mediaBrowserService.Update(Settings, artist);
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
