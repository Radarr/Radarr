using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class AlbumAddedEvent : IEvent
    {
        public Book Album { get; private set; }
        public bool DoRefresh { get; private set; }

        public AlbumAddedEvent(Book album, bool doRefresh = true)
        {
            Album = album;
            DoRefresh = doRefresh;
        }
    }
}
