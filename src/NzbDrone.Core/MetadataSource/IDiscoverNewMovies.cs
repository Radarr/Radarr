using System.Collections.Generic;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MetadataSource
{
    public interface IDiscoverNewMovies
    {
        List<Movie> DiscoverNewMovies(string action);
    }
}