using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class ArtistAddedEvent : IEvent
    {
        public Artist Artist { get; private set; }

        public ArtistAddedEvent(Artist artist)
        {
            Artist = artist;
        }
    }
}
