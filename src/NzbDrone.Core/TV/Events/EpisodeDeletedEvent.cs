using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.TV.Events
{
    public class EpisodeDeletedEvent : IEvent
    {
        public Episode Episode { get; private set; }
        public bool DeleteFiles { get; private set; }

        public EpisodeDeletedEvent(Episode episode, bool deleteFiles)
        {
            Episode = episode;
            DeleteFiles = deleteFiles;
        }
    }
}
