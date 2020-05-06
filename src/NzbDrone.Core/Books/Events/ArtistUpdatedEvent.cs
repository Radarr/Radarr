using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class ArtistUpdatedEvent : IEvent
    {
        public Author Artist { get; private set; }

        public ArtistUpdatedEvent(Author artist)
        {
            Artist = artist;
        }
    }
}
