using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.TV.Events
{
    public class TVShowAddedEvent : IEvent
    {
        public TVShow TVShow { get; private set; }

        public TVShowAddedEvent(TVShow tvShow)
        {
            TVShow = tvShow;
        }
    }
}
