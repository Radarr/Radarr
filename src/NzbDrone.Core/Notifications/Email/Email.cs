using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Email
{
    public class Email : NotificationBase<EmailSettings>
    {
        private readonly IEmailService _emailService;

        public override string Name => "Email";


        public Email(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public override string Link => null;

        public override void OnGrab(GrabMessage grabMessage)
        {
            var body = $"{grabMessage.Message} sent to queue.";

            _emailService.SendEmail(Settings, ALBUM_GRABBED_TITLE_BRANDED, body);
        }

        public override void OnDownload(TrackDownloadMessage message)
        {
            var body = $"{message.Message} Downloaded and sorted.";

            _emailService.SendEmail(Settings, TRACK_DOWNLOADED_TITLE_BRANDED, body);
        }

        public override void OnAlbumDownload(AlbumDownloadMessage message)
        {
            var body = $"{message.Message} Downloaded and sorted.";

            _emailService.SendEmail(Settings, ALBUM_DOWNLOADED_TITLE_BRANDED, body);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_emailService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
