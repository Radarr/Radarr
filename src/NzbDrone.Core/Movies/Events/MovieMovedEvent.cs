using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Events
{
    public class MovieMovedEvent : IEvent
    {
        public Movie Movie { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }

        public MovieMovedEvent(Movie movie, string sourcePath, string destinationPath)
        {
            Movie = movie;
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
        }
    }
}
