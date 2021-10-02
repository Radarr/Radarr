using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Simplepush
{
    public class Simplepush : NotificationBase<SimplepushSettings>
    {
        private readonly ISimplepushProxy _proxy;

        public Simplepush(ISimplepushProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Name => "Simplepush";
        public override string Link => "https://simplepush.io/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            _proxy.SendNotification(MOVIE_GRABBED_TITLE, grabMessage.Message, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _proxy.SendNotification(MOVIE_DOWNLOADED_TITLE, message.Message, Settings);
        }

        public override void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(MOVIE_FILE_DELETED_TITLE, deleteMessage.Message, Settings);
        }

        public override void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(MOVIE_DELETED_TITLE, deleteMessage.Message, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            _proxy.SendNotification(HEALTH_ISSUE_TITLE, healthCheck.Message, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
