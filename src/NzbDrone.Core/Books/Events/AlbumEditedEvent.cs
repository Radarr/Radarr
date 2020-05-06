using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class AlbumEditedEvent : IEvent
    {
        public Book Album { get; private set; }
        public Book OldAlbum { get; private set; }

        public AlbumEditedEvent(Book album, Book oldAlbum)
        {
            Album = album;
            OldAlbum = oldAlbum;
        }
    }
}
