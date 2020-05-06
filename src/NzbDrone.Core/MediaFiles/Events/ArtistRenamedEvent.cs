using NzbDrone.Common.Messaging;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class ArtistRenamedEvent : IEvent
    {
        public Author Artist { get; private set; }

        public ArtistRenamedEvent(Author artist)
        {
            Artist = artist;
        }
    }
}
