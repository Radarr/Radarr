using NzbDrone.Common.Messaging;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class AuthorScannedEvent : IEvent
    {
        public Author Author { get; private set; }

        public AuthorScannedEvent(Author author)
        {
            Author = author;
        }
    }
}
