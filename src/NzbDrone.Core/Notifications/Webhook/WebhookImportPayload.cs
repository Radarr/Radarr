using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookImportPayload : WebhookPayload
    {
        public List<WebhookTrack> Tracks { get; set; }
        public WebhookTrackFile TrackFile { get; set; }
        public bool IsUpgrade { get; set; }
    }
}
