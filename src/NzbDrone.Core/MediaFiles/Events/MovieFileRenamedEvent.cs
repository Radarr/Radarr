using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieFileRenamedEvent : IEvent
    {
        public Movie Movie { get; private set; }
        public MovieFile MovieFile { get; private set; }
        public string OriginalPath { get; private set; }

        public MovieFileRenamedEvent(Movie movie, MovieFile movieFile, string originalPath)
        {
            Movie = movie;
            MovieFile = movieFile;
            OriginalPath = originalPath;
        }
    }
}
