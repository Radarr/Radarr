using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaCover
{
    public class MediaCoversUpdatedEvent : IEvent
    {
        public Movie Movie { get; set; }

        public MediaCoversUpdatedEvent(Movie movie)
        {
            Movie = movie;
        }
    }
}
