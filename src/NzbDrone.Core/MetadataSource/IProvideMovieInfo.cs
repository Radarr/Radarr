using System;
using System.Collections.Generic;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Credits;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideMovieInfo
    {
        Movie GetMovieInfo(string imdbId);
        Tuple<Movie, List<Credit>> GetMovieInfo(int tmdbId, bool hasPreDBEntry);
        HashSet<int> GetChangedMovies(DateTime startTime);
    }
}
