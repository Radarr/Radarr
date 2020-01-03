using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class ReleaseDeletedEvent : IEvent
    {
        public AlbumRelease Release { get; private set; }

        public ReleaseDeletedEvent(AlbumRelease release)
        {
            Release = release;
        }
    }
}
