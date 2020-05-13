using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public class BookGrabbedEvent : IEvent
    {
        public RemoteBook Book { get; private set; }
        public string DownloadClient { get; set; }
        public string DownloadId { get; set; }

        public BookGrabbedEvent(RemoteBook book)
        {
            Book = book;
        }
    }
}
