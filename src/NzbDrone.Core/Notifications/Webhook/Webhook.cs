
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Core.Music;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class Webhook : NotificationBase<WebhookSettings>
    {
        private readonly IWebhookService _service;

        public Webhook(IWebhookService service)
        {
            _service = service;
        }

        public override string Link => "https://github.com/Lidarr/Lidarr/wiki/Webhook";

        public override void OnGrab(GrabMessage message)
        {
            _service.OnGrab(message.Artist, message.Album, message.Quality, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _service.OnDownload(message.Artist, message.TrackFile, Settings);
        }

        public override void OnRename(Artist artist)
        {
            _service.OnRename(artist, Settings);
        }

        public override string Name => "Webhook";

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_service.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
