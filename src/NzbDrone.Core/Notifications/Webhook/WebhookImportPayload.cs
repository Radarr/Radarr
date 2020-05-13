using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookImportPayload : WebhookPayload
    {
        public WebhookBook Book { get; set; }
        public List<WebhookBookFile> BookFiles { get; set; }
        public bool IsUpgrade { get; set; }
    }
}
