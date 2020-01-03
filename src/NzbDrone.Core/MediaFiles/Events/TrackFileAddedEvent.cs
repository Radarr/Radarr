using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class TrackFileAddedEvent : IEvent
    {
        public TrackFile TrackFile { get; private set; }

        public TrackFileAddedEvent(TrackFile trackFile)
        {
            TrackFile = trackFile;
        }
    }
}
