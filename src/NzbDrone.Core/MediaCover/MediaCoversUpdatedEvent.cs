using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaCover
{
    public class MediaCoversUpdatedEvent : IEvent
    {
        public Movie Movie { get; set; }
        public bool Updated { get; set; }

        // The poster was replaced or restored to the original
        public bool PosterReplacementChanged { get; set; }

        public MediaCoversUpdatedEvent(Movie movie, bool updated, bool posterReplacementChanged = false)
        {
            Movie = movie;
            Updated = updated;
            PosterReplacementChanged = posterReplacementChanged;
        }
    }
}
