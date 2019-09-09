using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Growl
{
    public class Growl : NotificationBase<GrowlSettings>
    {
        private readonly IGrowlService _growlService;

        public override string Name => "Growl";


        public Growl(IGrowlService growlService)
        {
            _growlService = growlService;
        }

        public override string Link => "http://growl.info/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            _growlService.SendNotification(ALBUM_GRABBED_TITLE, grabMessage.Message, "GRAB", Settings.Host, Settings.Port, Settings.Password);
        }

        public override void OnReleaseImport(AlbumDownloadMessage message)
        {
            _growlService.SendNotification(ALBUM_DOWNLOADED_TITLE, message.Message, "ALBUMDOWNLOAD", Settings.Host, Settings.Port, Settings.Password);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck message)
        {
            _growlService.SendNotification(HEALTH_ISSUE_TITLE, message.Message, "HEALTHISSUE", Settings.Host, Settings.Port, Settings.Password);
        }

        public override void OnDownloadFailure(DownloadFailedMessage message)
        {
            _growlService.SendNotification(DOWNLOAD_FAILURE_TITLE, message.Message, "DOWNLOADFAILURE", Settings.Host, Settings.Port, Settings.Password);
        }

        public override void OnImportFailure(AlbumDownloadMessage message)
        {
            _growlService.SendNotification(IMPORT_FAILURE_TITLE, message.Message, "IMPORTFAILURE", Settings.Host, Settings.Port, Settings.Password);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_growlService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
