using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.TV.Events
{
    public class SeasonEditedEvent : IEvent
    {
        public Season Season { get; private set; }
        public Season OldSeason { get; private set; }

        public SeasonEditedEvent(Season season, Season oldSeason)
        {
            Season = season;
            OldSeason = oldSeason;
        }
    }
}
