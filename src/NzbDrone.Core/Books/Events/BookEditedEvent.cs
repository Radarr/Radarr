using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Books.Events
{
    public class BookEditedEvent : IEvent
    {
        public Book Album { get; private set; }
        public Book OldAlbum { get; private set; }

        public BookEditedEvent(Book book, Book oldAlbum)
        {
            Album = book;
            OldAlbum = oldAlbum;
        }
    }
}
