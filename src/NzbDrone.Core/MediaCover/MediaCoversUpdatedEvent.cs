using NzbDrone.Common.Messaging;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaCover
{
    public class MediaCoversUpdatedEvent : IEvent
    {
        public Author Artist { get; set; }
        public Book Album { get; set; }

        public MediaCoversUpdatedEvent(Author artist)
        {
            Artist = artist;
        }

        public MediaCoversUpdatedEvent(Book album)
        {
            Album = album;
        }
    }
}
