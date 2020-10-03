using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookGrabPayload : WebhookPayload
    {
        public List<WebhookBook> Books { get; set; }
        public WebhookRelease Release { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadId { get; set; }
    }
}
