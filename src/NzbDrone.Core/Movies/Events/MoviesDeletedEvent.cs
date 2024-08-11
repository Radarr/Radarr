using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Movies.Events
{
    public class MoviesDeletedEvent : IEvent
    {
        public List<Movie> Movies { get; private set; }
        public bool DeleteFiles { get; private set; }
        public bool AddImportListExclusion { get; private set; }

        public MoviesDeletedEvent(List<Movie> movies, bool deleteFiles, bool addImportListExclusion)
        {
            Movies = movies;
            DeleteFiles = deleteFiles;
            AddImportListExclusion = addImportListExclusion;
        }
    }
}
