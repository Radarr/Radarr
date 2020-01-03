using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class ArtistEditedEvent : IEvent
    {
        public Artist Artist { get; private set; }
        public Artist OldArtist { get; private set; }

        public ArtistEditedEvent(Artist artist, Artist oldArtist)
        {
            Artist = artist;
            OldArtist = oldArtist;
        }
    }
}
