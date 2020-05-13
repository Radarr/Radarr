namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookPayload
    {
        public string EventType { get; set; }
        public WebhookAuthor Author { get; set; }
    }
}
