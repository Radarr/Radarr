using NzbDrone.Common.Messaging;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class TrackFolderCreatedEvent : IEvent
    {
        public Artist Artist { get; private set; }
        public TrackFile TrackFile { get; private set; }
        public string ArtistFolder { get; set; }
        public string AlbumFolder { get; set; }
        public string TrackFolder { get; set; }

        public TrackFolderCreatedEvent(Artist artist, TrackFile trackFile)
        {
            Artist = artist;
            TrackFile = trackFile;
        }
    }
}
