using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class TracksDeletedEvent : IEvent
    {
        public List<Track> Tracks { get; private set; }
        public bool DeleteFiles { get; private set; }

        public TracksDeletedEvent(List<Track> tracks, bool deleteFiles)
        {
            Tracks = tracks;
            DeleteFiles = deleteFiles;
        }
    }
}
