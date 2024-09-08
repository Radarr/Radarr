using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Gotify
{
    public class GotifyMessage
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public int Priority { get; set; }
        public GotifyExtras Extras { get; set; }

        public GotifyMessage()
        {
            Extras = new GotifyExtras();
        }

        public void SetBigImageUrl(string bigImageUrl)
        {
            Extras.ClientNotification = new GotifyClientNotification(bigImageUrl);
        }
    }

    public class GotifyExtras
    {
        [JsonProperty("client::notification")]
        public GotifyClientNotification ClientNotification { get; set; }
    }

    public class GotifyClientNotification
    {
        [JsonProperty("bigImageUrl")]
        public string BigImageUrl { get; set; }

        public GotifyClientNotification(string bigImageUrl)
        {
            BigImageUrl = bigImageUrl;
        }
    }
}
