using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class ArtistRefreshCompleteEvent : IEvent
    {
        public Artist Artist { get; set; }

        public ArtistRefreshCompleteEvent(Artist artist)
        {
            Artist = artist;
        }
    }
}
