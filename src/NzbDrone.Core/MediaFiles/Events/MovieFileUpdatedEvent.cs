using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieFileUpdatedEvent : IEvent
    {
        public MovieFile MovieFile { get; private set; }

        public MovieFileUpdatedEvent(MovieFile movieFile)
        {
            MovieFile = movieFile;
        }
    }
}
