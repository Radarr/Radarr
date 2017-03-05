
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Core.Tv;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class Webhook : NotificationBase<WebhookSettings>
    {
        private readonly IWebhookService _service;

        public Webhook(IWebhookService service)
        {
            _service = service;
        }

        public override string Link => "https://github.com/Sonarr/Sonarr/wiki/Webhook";

        public override void OnGrab(GrabMessage message)
        {
            _service.OnGrab(message.Movie, message.RemoteMovie, message.Quality, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            _service.OnDownload(message.Movie, message.MovieFile, Settings);
        }

        public override void OnMovieRename(Movie movie)
        {
            _service.OnRename(movie, Settings);
        }

        public override void OnRename(Series series)
        {
        }

        public override string Name => "Webhook";

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_service.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
