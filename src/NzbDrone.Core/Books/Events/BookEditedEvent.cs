using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Books.Events
{
    public class BookEditedEvent : IEvent
    {
        public Book Book { get; private set; }
        public Book OldBook { get; private set; }

        public BookEditedEvent(Book book, Book oldBook)
        {
            Book = book;
            OldBook = oldBook;
        }
    }
}
