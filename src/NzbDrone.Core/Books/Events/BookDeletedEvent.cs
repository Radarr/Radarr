using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Books.Events
{
    public class BookDeletedEvent : IEvent
    {
        public Book Book { get; private set; }
        public bool DeleteFiles { get; private set; }
        public bool AddImportListExclusion { get; private set; }

        public BookDeletedEvent(Book book, bool deleteFiles, bool addImportListExclusion)
        {
            Book = book;
            DeleteFiles = deleteFiles;
            AddImportListExclusion = addImportListExclusion;
        }
    }
}
