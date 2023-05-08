using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookAddedPayload : WebhookPayload
    {
        public WebhookMovie Movie { get; set; }
        public AddMovieMethod AddMethod { get; set; }
    }
}
