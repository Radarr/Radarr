using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.TV.Events
{
    public class SeasonDeletedEvent : IEvent
    {
        public Season Season { get; private set; }

        public SeasonDeletedEvent(Season season)
        {
            Season = season;
        }
    }
}
