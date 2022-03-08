namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookMovieDeletePayload : WebhookPayload
    {
        public WebhookMovie Movie { get; set; }
        public bool DeletedFiles { get; set; }
    }
}
