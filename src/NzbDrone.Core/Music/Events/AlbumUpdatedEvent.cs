using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class AlbumUpdatedEvent : IEvent
    {
        public Album Album { get; private set; }

        public AlbumUpdatedEvent(Album album)
        {
            Album = album;
        }
    }
}
