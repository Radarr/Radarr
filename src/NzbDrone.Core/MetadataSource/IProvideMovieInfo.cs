using System;
using System.Collections.Generic;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideMovieInfo
    {
        Movie GetMovieInfo(string ImdbId);
        Movie GetMovieInfo(int TmdbId, Profile profile, bool hasPreDBEntry);
    }
}