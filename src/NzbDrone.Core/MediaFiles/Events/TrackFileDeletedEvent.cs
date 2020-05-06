using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class TrackFileDeletedEvent : IEvent
    {
        public BookFile TrackFile { get; private set; }
        public DeleteMediaFileReason Reason { get; private set; }

        public TrackFileDeletedEvent(BookFile trackFile, DeleteMediaFileReason reason)
        {
            TrackFile = trackFile;
            Reason = reason;
        }
    }
}
