using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class AlbumEditedEvent : IEvent
    {
        public Album Album { get; private set; }
        public Album OldAlbum { get; private set; }

        public AlbumEditedEvent(Album album, Album oldAlbum)
        {
            Album = album;
            OldAlbum = oldAlbum;
        }
    }
}
