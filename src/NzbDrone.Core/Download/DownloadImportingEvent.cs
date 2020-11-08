using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download.TrackedDownloads;

namespace NzbDrone.Core.Download
{
    public class DownloadImportingEvent : IEvent
    {
        public TrackedDownload TrackedDownload { get; private set; }

        public DownloadImportingEvent(TrackedDownload trackedDownload)
        {
            TrackedDownload = trackedDownload;
        }
    }
}
