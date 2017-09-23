using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookPayload
    {
        public string EventType { get; set; }
        public WebhookArtist Artist { get; set; }
        public List<WebhookAlbum> Albums { get; set; }
    }
}
