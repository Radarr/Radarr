using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation.Results;
using NLog;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Gotify
{
    public class Gotify : NotificationBase<GotifySettings>
    {
        private readonly IGotifyProxy _proxy;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public Gotify(IGotifyProxy proxy, ILocalizationService localizationService, Logger logger)
        {
            _proxy = proxy;
            _localizationService = localizationService;
            _logger = logger;
        }

        public override string Name => "Gotify";
        public override string Link => "https://gotify.net/";

        public override void OnGrab(GrabMessage message)
        {
            SendNotification(MOVIE_GRABBED_TITLE, message.Message, message.Movie);
        }

        public override void OnDownload(DownloadMessage message)
        {
            SendNotification(MOVIE_DOWNLOADED_TITLE, message.Message, message.Movie);
        }

        public override void OnMovieAdded(Movie movie)
        {
            SendNotification(MOVIE_ADDED_TITLE, $"{movie.Title} added to library", movie);
        }

        public override void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            SendNotification(MOVIE_FILE_DELETED_TITLE, deleteMessage.Message, deleteMessage.Movie);
        }

        public override void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            SendNotification(MOVIE_DELETED_TITLE, deleteMessage.Message, deleteMessage.Movie);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            SendNotification(HEALTH_ISSUE_TITLE, healthCheck.Message, null);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            SendNotification(HEALTH_RESTORED_TITLE, $"The following issue is now resolved: {previousCheck.Message}", null);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage message)
        {
            SendNotification(APPLICATION_UPDATE_TITLE, message.Message, null);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            SendNotification(MANUAL_INTERACTION_REQUIRED_TITLE, message.Message, message.Movie);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            try
            {
                var isMarkdown = false;
                const string title = "Test Notification";

                var sb = new StringBuilder();
                sb.AppendLine("This is a test message from Radarr");

                if (Settings.IncludeMoviePoster)
                {
                    isMarkdown = true;

                    sb.AppendLine("\r![](https://raw.githubusercontent.com/Radarr/Radarr/develop/Logo/128.png)");
                }

                var payload = new GotifyMessage
                {
                    Title = title,
                    Message = sb.ToString(),
                    Priority = Settings.Priority
                };

                payload.SetContentType(isMarkdown);

                _proxy.SendNotification(payload, Settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                failures.Add(new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } })));
            }

            return new ValidationResult(failures);
        }

        private void SendNotification(string title, string message, Movie movie)
        {
            var isMarkdown = false;
            var sb = new StringBuilder();

            sb.AppendLine(message);

            if (Settings.IncludeMoviePoster && movie != null)
            {
                var poster = movie.MovieMetadata.Value.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl;

                if (poster != null)
                {
                    isMarkdown = true;
                    sb.AppendLine($"\r![]({poster})");
                }
            }

            var payload = new GotifyMessage
            {
                Title = title,
                Message = sb.ToString(),
                Priority = Settings.Priority
            };

            payload.SetContentType(isMarkdown);

            _proxy.SendNotification(payload, Settings);
        }
    }
}
