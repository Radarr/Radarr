using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using Prowlin;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class Prowl : NotificationBase<ProwlSettings>
    {
        private readonly IProwlService _prowlService;

        public Prowl(IProwlService prowlService)
        {
            _prowlService = prowlService;
        }

        public override string Link => "https://www.prowlapp.com/";
        public override string Name => "Prowl";

        public override void OnGrab(GrabMessage grabMessage)
        {
            _prowlService.SendNotification(ALBUM_GRABBED_TITLE, grabMessage.Message, Settings.ApiKey, (NotificationPriority)Settings.Priority);
        }

        public override void OnReleaseImport(AlbumDownloadMessage message)
        {
            _prowlService.SendNotification(ALBUM_DOWNLOADED_TITLE, message.Message, Settings.ApiKey, (NotificationPriority)Settings.Priority);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck message)
        {
            _prowlService.SendNotification(HEALTH_ISSUE_TITLE, message.Message, Settings.ApiKey, (NotificationPriority)Settings.Priority);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_prowlService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
