using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookGrabPayload : WebhookPayload
    {
        public List<WebhookAlbum> Albums { get; set; }
        public WebhookRelease Release { get; set; }
    }
}
