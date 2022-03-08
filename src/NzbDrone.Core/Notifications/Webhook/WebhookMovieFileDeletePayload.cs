using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookMovieFileDeletePayload : WebhookPayload
    {
        public WebhookMovie Movie { get; set; }
        public WebhookMovieFile MovieFile { get; set; }
        public DeleteMediaFileReason DeleteReason { get; set; }
    }
}
