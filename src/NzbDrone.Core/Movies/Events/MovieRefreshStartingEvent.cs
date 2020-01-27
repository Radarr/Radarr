using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Events
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
