namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookGrabPayload : WebhookPayload
    {
        public WebhookMovie Movie { get; set; }
        public WebhookRemoteMovie RemoteMovie { get; set; }
        public WebhookRelease Release { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadClientType { get; set; }
        public string DownloadId { get; set; }
    }
}
