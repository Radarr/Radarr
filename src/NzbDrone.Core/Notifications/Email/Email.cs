using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Email
{
    public class Email : NotificationBase<EmailSettings>
    {
        private readonly IEmailService _emailService;

        public Email(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public override string Link => null;

        public override void OnGrab(GrabMessage grabMessage)
        {
            const string subject = "Radarr [Movie] - Grabbed";
            var body = string.Format("{0} sent to queue.", grabMessage.Message);
            
            if (grabMessage.RemoteMovie.Release.InfoUrl.IsNotNullOrWhiteSpace())
                body = $"{body}\n Release info: {grabMessage.RemoteMovie.Release.InfoUrl}";

            _emailService.SendEmail(Settings, subject, body);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string subject = "Radarr [Movie] - Downloaded";
            var body = string.Format("{0} Downloaded and sorted.", message.Message);

            _emailService.SendEmail(Settings, subject, body);
        }
		
        public override void OnMovieRename(Movie movie)
        {
        }
		
        public override string Name => "Email";

        public override bool SupportsOnRename => false;

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_emailService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
