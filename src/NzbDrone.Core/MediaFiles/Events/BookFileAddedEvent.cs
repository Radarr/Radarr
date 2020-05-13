using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class BookFileAddedEvent : IEvent
    {
        public BookFile BookFile { get; private set; }

        public BookFileAddedEvent(BookFile bookFile)
        {
            BookFile = bookFile;
        }
    }
}
