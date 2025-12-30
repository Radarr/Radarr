using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.TV.Events
{
    public class EpisodeAddedEvent : IEvent
    {
        public Episode Episode { get; private set; }

        public EpisodeAddedEvent(Episode episode)
        {
            Episode = episode;
        }
    }
}
