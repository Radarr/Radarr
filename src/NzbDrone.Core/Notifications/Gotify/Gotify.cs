using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Gotify
{
    public class Gotify : NotificationBase<GotifySettings>
    {
        private readonly IGotifyProxy _proxy;
        private readonly Logger _logger;

        public Gotify(IGotifyProxy proxy, Logger logger)
        {
            _proxy = proxy;
            _logger = logger;
        }

        public override string Name => "Gotify";
        public override string Link => "https://gotify.net/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            const string title = "Movie Grabbed";

            _proxy.SendNotification(title, grabMessage.Message, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string title = "Movie Downloaded";

            _proxy.SendNotification(title, message.Message, Settings);
        }

        public override void OnMovieRename(Movie movie)
        {
        }

        public override bool SupportsOnRename => false;

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            try
            {
                const string title = "Test Notification";
                const string body = "This is a test message from Radarr";

                _proxy.SendNotification(title, body, Settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                failures.Add(new ValidationFailure("", "Unable to send test message"));
            }

            return new ValidationResult(failures);
        }
    }
}
