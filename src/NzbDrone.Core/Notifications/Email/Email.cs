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

            _emailService.SendEmail(Settings, MOVIE_GRABBED_TITLE_BRANDED, body);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var body = $"{message.Message} Downloaded and sorted.";

            _emailService.SendEmail(Settings, MOVIE_DOWNLOADED_TITLE_BRANDED, body);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck message)
        {
            _emailService.SendEmail(Settings, HEALTH_ISSUE_TITLE_BRANDED, message.Message);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_emailService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
