using System;
using System.Collections.Generic;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideMovieInfo
    {
        Movie GetMovieInfo(string imdbId);
        Movie GetMovieInfo(int tmdbId, Profile profile, bool hasPreDBEntry);
        HashSet<int> GetChangedMovies(DateTime startTime);
    }
}
