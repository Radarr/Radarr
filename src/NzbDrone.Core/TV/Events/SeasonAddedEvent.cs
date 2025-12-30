using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.TV.Events
{
    public class SeasonAddedEvent : IEvent
    {
        public Season Season { get; private set; }

        public SeasonAddedEvent(Season season)
        {
            Season = season;
        }
    }
}
