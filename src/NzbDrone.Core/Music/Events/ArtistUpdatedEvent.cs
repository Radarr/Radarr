using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class ArtistUpdatedEvent : IEvent
    {
        public Artist Artist { get; private set; }

        public ArtistUpdatedEvent(Artist artist)
        {
            Artist = artist;
        }
    }
}
