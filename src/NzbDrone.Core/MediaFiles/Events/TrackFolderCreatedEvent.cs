using NzbDrone.Common.Messaging;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class TrackFolderCreatedEvent : IEvent
    {
        public Author Artist { get; private set; }
        public BookFile TrackFile { get; private set; }
        public string ArtistFolder { get; set; }
        public string AlbumFolder { get; set; }
        public string TrackFolder { get; set; }

        public TrackFolderCreatedEvent(Author artist, BookFile trackFile)
        {
            Artist = artist;
            TrackFile = trackFile;
        }
    }
}
