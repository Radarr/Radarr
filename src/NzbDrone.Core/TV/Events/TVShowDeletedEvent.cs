using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.TV.Events
{
    public class TVShowDeletedEvent : IEvent
    {
        public TVShow TVShow { get; private set; }
        public bool DeleteFiles { get; private set; }

        public TVShowDeletedEvent(TVShow tvShow, bool deleteFiles)
        {
            TVShow = tvShow;
            DeleteFiles = deleteFiles;
        }
    }
}
