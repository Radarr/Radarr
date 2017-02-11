using System.Collections.Generic;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewMovie
    {
        List<Movie> SearchForNewMovie(string title);

        Movie MapMovieToTmdbMovie(Movie movie);
    }
}