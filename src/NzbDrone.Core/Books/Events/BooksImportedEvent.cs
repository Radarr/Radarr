using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Books.Events
{
    public class BooksImportedEvent : IEvent
    {
        public List<Book> Books { get; private set; }

        public BooksImportedEvent(List<Book> books)
        {
            Books = books;
        }
    }
}
