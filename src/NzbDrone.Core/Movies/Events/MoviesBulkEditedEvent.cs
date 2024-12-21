using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Events
{
    public class MoviesBulkEditedEvent : IEvent
    {
        public IReadOnlyCollection<Movie> Movies { get; private set; }

        public MoviesBulkEditedEvent(IReadOnlyCollection<Movie> movies)
        {
            Movies = movies;
        }
    }
}
