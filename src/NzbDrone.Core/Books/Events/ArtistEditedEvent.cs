using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class ArtistEditedEvent : IEvent
    {
        public Author Artist { get; private set; }
        public Author OldArtist { get; private set; }

        public ArtistEditedEvent(Author artist, Author oldArtist)
        {
            Artist = artist;
            OldArtist = oldArtist;
        }
    }
}
