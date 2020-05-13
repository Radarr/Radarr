using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Books.Events
{
    public class BookAddedEvent : IEvent
    {
        public Book Book { get; private set; }
        public bool DoRefresh { get; private set; }

        public BookAddedEvent(Book book, bool doRefresh = true)
        {
            Book = book;
            DoRefresh = doRefresh;
        }
    }
}
