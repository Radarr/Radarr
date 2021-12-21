using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Join
{
    public class Join : NotificationBase<JoinSettings>
    {
        private readonly IJoinProxy _proxy;

        public Join(IJoinProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Name => "Join";

        public override string Link => "https://joaoapps.com/join/";

        public override void OnGrab(GrabMessage message)
        {
            _proxy.SendNotification(MOVIE_GRABBED_TITLE_BRANDED, message.Message, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _proxy.SendNotification(MOVIE_DOWNLOADED_TITLE_BRANDED, message.Message, Settings);
        }

        public override void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(MOVIE_FILE_DELETED_TITLE_BRANDED, deleteMessage.Message, Settings);
        }

        public override void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            _proxy.SendNotification(MOVIE_DELETED_TITLE_BRANDED, deleteMessage.Message, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck message)
        {
            _proxy.SendNotification(HEALTH_ISSUE_TITLE_BRANDED, message.Message, Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            _proxy.SendNotification(APPLICATION_UPDATE_TITLE_BRANDED, updateMessage.Message, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
