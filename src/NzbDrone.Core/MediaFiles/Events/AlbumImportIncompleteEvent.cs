using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download.TrackedDownloads;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class AlbumImportIncompleteEvent : IEvent
    {
        public TrackedDownload TrackedDownload { get; private set; }

        public AlbumImportIncompleteEvent(TrackedDownload trackedDownload)
        {
            TrackedDownload = trackedDownload;
        }
    }
}
