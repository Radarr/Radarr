namespace NzbDrone.Core.Notifications.Webhook
{
    class WebhookGrabPayload : WebhookPayload
    {
        public WebhookRemoteMovie RemoteMovie { get; set; }
        public WebhookRelease Release { get; set; }
    }
}
