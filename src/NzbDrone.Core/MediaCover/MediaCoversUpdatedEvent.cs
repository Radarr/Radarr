using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaCover
{
    public class MediaCoversUpdatedEvent : IEvent
    {
        public Movie Movie { get; set; }
        public bool Updated { get; set; }


        public MediaCoversUpdatedEvent(Movie movie, bool updated)
        {
            Movie = movie;
            Updated = updated;
        }
    }
}
