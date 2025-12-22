using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Books.Events
{
    public class BooksDeletedEvent : IEvent
    {
        public List<Book> Books { get; private set; }
        public bool DeleteFiles { get; private set; }

        public BooksDeletedEvent(List<Book> books, bool deleteFiles)
        {
            Books = books;
            DeleteFiles = deleteFiles;
        }
    }
}
