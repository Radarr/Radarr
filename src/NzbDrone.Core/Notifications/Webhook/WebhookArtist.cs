using NzbDrone.Core.Music;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookArtist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string MBId { get; set; }

        public WebhookArtist() { }

        public WebhookArtist(Artist artist)
        {
            Id = artist.Id;
            Name = artist.Name;
            Path = artist.Path;
            MBId = artist.Metadata.Value.ForeignArtistId;
        }
    }
}
