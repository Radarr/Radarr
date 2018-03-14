using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Events
{
    public class MovieAddedEvent : IEvent
    {
        public Movie Movie { get; private set; }

        public MovieAddedEvent(Movie movie)
        {
            Movie = movie;
        }
    }
}
