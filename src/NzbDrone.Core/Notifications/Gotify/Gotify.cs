using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Gotify
{
    public class Gotify : NotificationBase<GotifySettings>
    {
        private const string RadarrImageUrl = "https://raw.githubusercontent.com/Radarr/Radarr/develop/Logo/128.png";

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

                var payload = new GotifyMessage
                {
                    Title = title,
                    Priority = Settings.Priority
                };

                if (Settings.IncludeMoviePoster)
                {
                    isMarkdown = true;

                    sb.AppendLine($"\r![]({RadarrImageUrl})");
                    payload.SetImage(RadarrImageUrl);
                }

                if (Settings.MetadataLinks.Any())
                {
                    isMarkdown = true;

                    sb.AppendLine("");
                    sb.AppendLine("[Radarr.video](https://radarr.video)");
                    payload.SetClickUrl("https://radarr.video");
                }

                payload.Message = sb.ToString();
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

            var payload = new GotifyMessage
            {
                Title = title,
                Priority = Settings.Priority
            };

            if (movie != null)
            {
                if (Settings.IncludeMoviePoster)
                {
                    var poster = movie.MovieMetadata.Value.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl;

                    if (poster != null)
                    {
                        isMarkdown = true;
                        sb.AppendLine($"\r![]({poster})");
                        payload.SetImage(poster);
                    }
                }

                if (Settings.MetadataLinks.Any())
                {
                    isMarkdown = true;
                    sb.AppendLine("");

                    foreach (var link in Settings.MetadataLinks)
                    {
                        var linkType = (MetadataLinkType)link;
                        var linkText = "";
                        var linkUrl = "";

                        if (linkType == MetadataLinkType.Tmdb && movie.TmdbId > 0)
                        {
                            linkText = "TMDb";
                            linkUrl = $"https://www.themoviedb.org/movie/{movie.TmdbId}";
                        }

                        if (linkType == MetadataLinkType.Imdb && movie.ImdbId.IsNotNullOrWhiteSpace())
                        {
                            linkText = "IMDb";
                            linkUrl = $"https://www.imdb.com/title/{movie.ImdbId}";
                        }

                        if (linkType == MetadataLinkType.Trakt && movie.TmdbId > 0)
                        {
                            linkText = "Trakt";
                            linkUrl = $"https://trakt.tv/search/tmdb/{movie.TmdbId}?id_type=movie";
                        }

                        sb.AppendLine($"[{linkText}]({linkUrl})");

                        if (link == Settings.PreferredMetadataLink)
                        {
                            payload.SetClickUrl(linkUrl);
                        }
                    }
                }
            }

            payload.Message = sb.ToString();
            payload.SetContentType(isMarkdown);

            _proxy.SendNotification(payload, Settings);
        }
    }
}
