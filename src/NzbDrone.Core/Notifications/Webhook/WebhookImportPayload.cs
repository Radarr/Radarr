using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookImportPayload : WebhookPayload
    {
        public WebhookAlbum Book { get; set; }
        public List<WebhookTrackFile> TrackFiles { get; set; }
        public bool IsUpgrade { get; set; }
    }
}
