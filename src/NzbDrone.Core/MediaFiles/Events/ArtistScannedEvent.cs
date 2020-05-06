using NzbDrone.Common.Messaging;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class ArtistScannedEvent : IEvent
    {
        public Author Artist { get; private set; }

        public ArtistScannedEvent(Author artist)
        {
            Artist = artist;
        }
    }
}
