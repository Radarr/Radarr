using NzbDrone.Common.Messaging;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaCover
{
    public class MediaCoversUpdatedEvent : IEvent
    {
        public Artist Artist { get; set; }
        public Album Album { get; set; }

        public MediaCoversUpdatedEvent(Artist artist)
        {
            Artist = artist;
        }

        public MediaCoversUpdatedEvent(Album album)
        {
            Album = album;
        }
    }
}
