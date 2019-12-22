namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookImportPayload : WebhookPayload
    {
        public WebhookRemoteMovie RemoteMovie { get; set; }
        public WebhookMovieFile MovieFile { get; set; }
        public bool IsUpgrade { get; set; }
    }
}
