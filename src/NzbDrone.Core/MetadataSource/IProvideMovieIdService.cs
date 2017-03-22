using System;
using System.Collections.Generic;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideMovieIdService
    {
        dynamic GetImdbIdByTmdbId(int TmdbId, bool includeTT = false);
        int GetTmdbIdByImdbId(string ImdbId);
    }
}