using NzbDrone.Common.Messaging;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class TrackFolderCreatedEvent : IEvent
    {
        public Author Author { get; private set; }
        public BookFile BookFile { get; private set; }
        public string AuthorFolder { get; set; }
        public string BookFolder { get; set; }
        public string TrackFolder { get; set; }

        public TrackFolderCreatedEvent(Author author, BookFile bookFile)
        {
            Author = author;
            BookFile = bookFile;
        }
    }
}
