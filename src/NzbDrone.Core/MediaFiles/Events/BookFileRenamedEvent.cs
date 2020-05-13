using NzbDrone.Common.Messaging;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class BookFileRenamedEvent : IEvent
    {
        public Author Author { get; private set; }
        public BookFile BookFile { get; private set; }
        public string OriginalPath { get; private set; }

        public BookFileRenamedEvent(Author author, BookFile bookFile, string originalPath)
        {
            Author = author;
            BookFile = bookFile;
            OriginalPath = originalPath;
        }
    }
}
