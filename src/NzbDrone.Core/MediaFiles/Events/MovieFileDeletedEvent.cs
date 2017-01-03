using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieFileDeletedEvent : IEvent
    {
        public MovieFile MovieFile { get; private set; }
        public DeleteMediaFileReason Reason { get; private set; }

        public MovieFileDeletedEvent(MovieFile episodeFile, DeleteMediaFileReason reason)
        {
            MovieFile = episodeFile;
            Reason = reason;
        }
    }
}