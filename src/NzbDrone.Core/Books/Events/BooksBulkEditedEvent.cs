using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Books.Events
{
    public class BooksBulkEditedEvent : IEvent
    {
        public IReadOnlyCollection<Book> Books { get; private set; }

        public BooksBulkEditedEvent(IReadOnlyCollection<Book> books)
        {
            Books = books;
        }
    }
}
