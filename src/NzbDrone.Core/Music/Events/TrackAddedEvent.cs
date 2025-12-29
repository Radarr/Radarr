using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class TrackAddedEvent : IEvent
    {
        public Track Track { get; private set; }

        public TrackAddedEvent(Track track)
        {
            Track = track;
        }
    }
}
