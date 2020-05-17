using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Credits;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideMovieInfo
    {
        Movie GetMovieByImdbId(string imdbId);
        Tuple<Movie, List<Credit>> GetMovieInfo(int tmdbId);
        HashSet<int> GetChangedMovies(DateTime startTime);

        Task<Tuple<Movie, List<Credit>>> GetMovieInfoAsync(int tmdbId);
    }
}
