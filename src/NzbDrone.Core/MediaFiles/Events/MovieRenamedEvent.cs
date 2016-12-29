using NzbDrone.Common.Messaging;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieRenamedEvent : IEvent
    {
        public Movie Movie { get; private set; }

        public MovieRenamedEvent(Movie movie)
        {
            Movie = movie;
        }
    }
}