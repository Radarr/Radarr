using NzbDrone.Core.Music;
using System;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookTrack
    {
        public WebhookTrack() { }

        public WebhookTrack(Track track)
        {
            Id = track.Id;
            Title = track.Title;
            TrackNumber = track.TrackNumber;
            
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string TrackNumber { get; set; }

        public string Quality { get; set; }
        public int QualityVersion { get; set; }
        public string ReleaseGroup { get; set; }
    }
}
