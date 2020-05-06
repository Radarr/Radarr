using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class ArtistRefreshCompleteEvent : IEvent
    {
        public Author Artist { get; set; }

        public ArtistRefreshCompleteEvent(Author artist)
        {
            Artist = artist;
        }
    }
}
