using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Books.Events
{
    public class BookEditedEvent : IEvent
    {
        public Book Book { get; private set; }
        public Book OldAlbum { get; private set; }

        public BookEditedEvent(Book book, Book oldAlbum)
        {
            Book = book;
            OldAlbum = oldAlbum;
        }
    }
}
