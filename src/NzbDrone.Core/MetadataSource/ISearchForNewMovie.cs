using System.Collections.Generic;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewMovie
    {
        List<Movie> SearchForNewMovie(string title);

        MovieMetadata MapMovieToTmdbMovie(MovieMetadata movie);
    }
}
