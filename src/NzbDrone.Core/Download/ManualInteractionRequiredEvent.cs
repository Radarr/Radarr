using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public class ManualInteractionRequiredEvent : IEvent
    {
        public RemoteMovie RemoteMovie { get; private set; }
        public TrackedDownload TrackedDownload { get; private set; }
        public GrabbedReleaseInfo Release { get; private set; }

        public ManualInteractionRequiredEvent(TrackedDownload trackedDownload, GrabbedReleaseInfo release)
        {
            TrackedDownload = trackedDownload;
            RemoteMovie = trackedDownload.RemoteMovie;
            Release = release;
        }
    }
}
