using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Telegram
{
    public class Telegram : NotificationBase<TelegramSettings>
    {
        private readonly ITelegramProxy _proxy;

        public Telegram(ITelegramProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Name => "Telegram";
        public override string Link => "https://telegram.org/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            _proxy.SendNotification(ALBUM_GRABBED_TITLE, grabMessage.Message, Settings);
        }

        public override void OnDownload(TrackDownloadMessage message)
        {
            _proxy.SendNotification(TRACK_DOWNLOADED_TITLE, message.Message, Settings);
        }

        public override void OnAlbumDownload(AlbumDownloadMessage message)
        {
            _proxy.SendNotification(TRACK_DOWNLOADED_TITLE, message.Message, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
