using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieFileDeletedEvent : IEvent
    {
        public MovieFile MovieFile { get; private set; }
        public DeleteMediaFileReason Reason { get; private set; }

        public MovieFileDeletedEvent(MovieFile movieFile, DeleteMediaFileReason reason)
        {
            MovieFile = movieFile;
            Reason = reason;
        }
    }
}
