using System;
using System.Collections.Generic;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Credits;
using NzbDrone.Core.Movies.Translations;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideMovieInfo
    {
        Movie GetMovieByImdbId(string imdbId);
        Tuple<Movie, List<Credit>> GetMovieInfo(int tmdbId);
        List<Movie> GetBulkMovieInfo(List<int> tmdbIds);

        HashSet<int> GetChangedMovies(DateTime startTime);
    }
}
