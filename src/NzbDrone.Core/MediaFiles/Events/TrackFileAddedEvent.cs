using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class TrackFileAddedEvent : IEvent
    {
        public BookFile TrackFile { get; private set; }

        public TrackFileAddedEvent(BookFile trackFile)
        {
            TrackFile = trackFile;
        }
    }
}
