using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Music.Events
{
    public class ArtistMovedEvent : IEvent
    {
        public Author Artist { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }

        public ArtistMovedEvent(Author artist, string sourcePath, string destinationPath)
        {
            Artist = artist;
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
        }
    }
}
