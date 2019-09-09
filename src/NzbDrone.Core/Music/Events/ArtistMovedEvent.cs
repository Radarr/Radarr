using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class ArtistMovedEvent : IEvent
    {
        public Artist Artist { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }

        public ArtistMovedEvent(Artist artist, string sourcePath, string destinationPath)
        {
            Artist = artist;
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
        }
    }
}
