using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download.TrackedDownloads;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class BookImportIncompleteEvent : IEvent
    {
        public TrackedDownload TrackedDownload { get; private set; }

        public BookImportIncompleteEvent(TrackedDownload trackedDownload)
        {
            TrackedDownload = trackedDownload;
        }
    }
}
