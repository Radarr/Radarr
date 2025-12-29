using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class ArtistDeletedEvent : IEvent
    {
        public Artist Artist { get; private set; }
        public bool DeleteFiles { get; private set; }

        public ArtistDeletedEvent(Artist artist, bool deleteFiles)
        {
            Artist = artist;
            DeleteFiles = deleteFiles;
        }
    }
}
