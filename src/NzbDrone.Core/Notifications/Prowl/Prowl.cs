using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class Prowl : NotificationBase<ProwlSettings>
    {
        private readonly IProwlProxy _prowlProxy;

        public Prowl(IProwlProxy prowlProxy)
        {
            _prowlProxy = prowlProxy;
        }

        public override string Link => "https://www.prowlapp.com/";
        public override string Name => "Prowl";

        public override void OnGrab(GrabMessage message)
        {
            _prowlProxy.SendNotification(BOOK_GRABBED_TITLE, message.Message, Settings.ApiKey);
        }

        public override void OnReleaseImport(BookDownloadMessage message)
        {
            _prowlProxy.SendNotification(BOOK_DOWNLOADED_TITLE, message.Message, Settings.ApiKey);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            _prowlProxy.SendNotification(HEALTH_ISSUE_TITLE, healthCheck.Message, Settings.ApiKey, (ProwlPriority)Settings.Priority);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_prowlProxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
