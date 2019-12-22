
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.NotifyMyAndroid
{
    public class NotifyMyAndroid : NotificationBase<NotifyMyAndroidSettings>
    {
        private readonly INotifyMyAndroidProxy _proxy;

        public NotifyMyAndroid(INotifyMyAndroidProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Link => "http://www.notifymyandroid.com/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            const string title = "Movie Grabbed";

            _proxy.SendNotification(title, grabMessage.Message, Settings.ApiKey, (NotifyMyAndroidPriority)Settings.Priority);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string title = "Movie Downloaded";

            _proxy.SendNotification(title, message.Message, Settings.ApiKey, (NotifyMyAndroidPriority)Settings.Priority);
        }

        public override void OnMovieRename(Movie movie)
        {
        }

        public override string Name => "Notify My Android";

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
