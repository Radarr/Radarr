using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookRenamePayload : WebhookPayload
    {
        public WebhookMovie Movie { get; set; }
        public List<WebhookRenamedMovieFile> RenamedMovieFiles { get; set; }
    }
}
