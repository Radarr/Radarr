using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class AlbumUpdatedEvent : IEvent
    {
        public Book Album { get; private set; }

        public AlbumUpdatedEvent(Book album)
        {
            Album = album;
        }
    }
}
