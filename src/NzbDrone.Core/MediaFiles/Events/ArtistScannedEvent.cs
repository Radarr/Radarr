using NzbDrone.Common.Messaging;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class ArtistScannedEvent : IEvent
    {
        public Artist Artist { get; private set; }

        public ArtistScannedEvent(Artist artist)
        {
            Artist = artist;
        }
    }
}
