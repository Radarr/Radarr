using System.Collections.Generic;
using System.Threading.Tasks;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewMovie
    {
        Task<List<Movie>> SearchForNewMovie(string title);

        Task<MovieMetadata> MapMovieToTmdbMovie(MovieMetadata movie);
    }
}
