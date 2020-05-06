using System;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookAlbum
    {
        public WebhookAlbum()
        {
        }

        public WebhookAlbum(Book album)
        {
            Id = album.Id;
            Title = album.Title;
            ReleaseDate = album.ReleaseDate;
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime? ReleaseDate { get; set; }

        public string Quality { get; set; }
        public int QualityVersion { get; set; }
        public string ReleaseGroup { get; set; }
        public string SceneName { get; set; }
    }
}
