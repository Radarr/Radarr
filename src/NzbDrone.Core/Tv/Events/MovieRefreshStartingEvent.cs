using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Tv.Events
{
    public class MovieRefreshStartingEvent : IEvent
    {
        public bool ManualTrigger { get; set; }

        public MovieRefreshStartingEvent(bool manualTrigger)
        {
            ManualTrigger = manualTrigger;
        }
    }
}