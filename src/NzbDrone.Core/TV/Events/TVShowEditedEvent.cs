using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.TV.Events
{
    public class TVShowEditedEvent : IEvent
    {
        public TVShow TVShow { get; private set; }
        public TVShow OldTVShow { get; private set; }

        public TVShowEditedEvent(TVShow tvShow, TVShow oldTVShow)
        {
            TVShow = tvShow;
            OldTVShow = oldTVShow;
        }
    }
}
