using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class Webhook : NotificationBase<WebhookSettings>
    {
        private readonly IWebhookProxy _proxy;

        public Webhook(IWebhookProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Link => "https://github.com/Radarr/Radarr/wiki/Webhook";

        public override void OnGrab(GrabMessage message)
        {
            var remoteMovie = message.RemoteMovie;
            var quality = message.Quality;

            var payload = new WebhookGrabPayload
            {
                EventType = "Grab",
                Movie = new WebhookMovie(message.Movie),
                RemoteMovie = new WebhookRemoteMovie(remoteMovie),
                Release = new WebhookRelease(quality, remoteMovie),
                DownloadClient = message.DownloadClient,
                DownloadId = message.DownloadId
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var movieFile = message.MovieFile;

            var payload = new WebhookImportPayload
            {
                EventType = "Download",
                Movie = new WebhookMovie(message.Movie),
                RemoteMovie = new WebhookRemoteMovie(message.Movie),
                MovieFile = new WebhookMovieFile(movieFile),
                IsUpgrade = message.OldMovieFiles.Any(),
                DownloadClient = message.DownloadClient,
                DownloadId = message.DownloadId
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override void OnMovieRename(Movie movie)
        {
            var payload = new WebhookPayload
            {
                EventType = "Rename",
                Movie = new WebhookMovie(movie)
            };

            _proxy.SendWebhook(payload, Settings);
        }

        public override string Name => "Webhook";

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(SendWebhookTest());

            return new ValidationResult(failures);
        }

        private ValidationFailure SendWebhookTest()
        {
            try
            {
                var payload = new WebhookGrabPayload
                {
                    EventType = "Test",
                    Movie = new WebhookMovie
                    {
                        Id = 1,
                        Title = "Test Title",
                        FolderPath = "C:\\testpath",
                        ReleaseDate = "1970-01-01"
                    },
                    RemoteMovie = new WebhookRemoteMovie
                    {
                        TmdbId = 1234,
                        ImdbId = "5678",
                        Title = "Test title",
                        Year = 1970
                    },
                    Release = new WebhookRelease
                    {
                        Indexer = "Test Indexer",
                        Quality = "Test Quality",
                        QualityVersion = 1,
                        ReleaseGroup = "Test Group",
                        ReleaseTitle = "Test Title",
                        Size = 9999999
                    }
                };

                _proxy.SendWebhook(payload, Settings);
            }
            catch (WebhookException ex)
            {
                return new NzbDroneValidationFailure("Url", ex.Message);
            }

            return null;
        }
    }
}
