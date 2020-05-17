using System.Collections.Generic;
using System.Threading.Tasks;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewMovie
    {
        List<Movie> SearchForNewMovie(string title);

        Movie MapMovieToTmdbMovie(Movie movie);
        Task<Movie> MapMovieToTmdbMovieAsync(Movie movie);
    }
}
