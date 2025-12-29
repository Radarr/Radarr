using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.TV.Events
{
    public class EpisodeEditedEvent : IEvent
    {
        public Episode Episode { get; private set; }
        public Episode OldEpisode { get; private set; }

        public EpisodeEditedEvent(Episode episode, Episode oldEpisode)
        {
            Episode = episode;
            OldEpisode = oldEpisode;
        }
    }
}
