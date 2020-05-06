using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class ArtistAddedEvent : IEvent
    {
        public Author Artist { get; private set; }
        public bool DoRefresh { get; private set; }

        public ArtistAddedEvent(Author artist, bool doRefresh = true)
        {
            Artist = artist;
            DoRefresh = doRefresh;
        }
    }
}
