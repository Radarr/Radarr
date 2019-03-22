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

        public override void OnReleaseImport(AlbumDownloadMessage message)
        {
            var body = $"{message.Message} Downloaded and sorted.";

            _emailService.SendEmail(Settings, ALBUM_DOWNLOADED_TITLE_BRANDED, body);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck message)
        {
            _emailService.SendEmail(Settings, HEALTH_ISSUE_TITLE_BRANDED, message.Message);
        }

        public override void OnDownloadFailure(DownloadFailedMessage message)
        {
            _emailService.SendEmail(Settings, DOWNLOAD_FAILURE_TITLE_BRANDED, message.Message);
        }

        public override void OnImportFailure(AlbumDownloadMessage message)
        {
            _emailService.SendEmail(Settings, IMPORT_FAILURE_TITLE_BRANDED, message.Message);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_emailService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
