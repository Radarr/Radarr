using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Books.Events
{
    public class AuthorDeletedEvent : IEvent
    {
        public Author Author { get; private set; }
        public bool DeleteFiles { get; private set; }
        public bool AddImportListExclusion { get; private set; }

        public AuthorDeletedEvent(Author author, bool deleteFiles, bool addImportListExclusion)
        {
            Author = author;
            DeleteFiles = deleteFiles;
            AddImportListExclusion = addImportListExclusion;
        }
    }
}
