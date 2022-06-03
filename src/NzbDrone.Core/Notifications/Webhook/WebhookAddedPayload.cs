using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookAddedPayload : WebhookPayload
    {
        public WebhookMovie Movie { get; set; }
    }
}
