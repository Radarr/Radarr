using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class AlbumAddedEvent : IEvent
    {
        public Album Album { get; private set; }

        public AlbumAddedEvent(Album album)
        {
            Album = album;
        }
    }
}
