using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookApplicationUpdatePayload : WebhookPayload
    {
        public string Message { get; set; }
        public string PreviousVersion { get; set; }
        public string NewVersion { get; set; }
        public List<string> NewChanges { get; set; }
        public List<string> FixedChanges { get; set; }
    }
}
