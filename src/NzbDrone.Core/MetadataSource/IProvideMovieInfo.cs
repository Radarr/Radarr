using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Collections;
using NzbDrone.Core.Movies.Credits;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideMovieInfo
    {
        Task<MovieMetadata> GetMovieByImdbId(string imdbId);
        Task<Tuple<MovieMetadata, List<Credit>>> GetMovieInfo(int tmdbId);
        MovieCollection GetCollectionInfo(int tmdbId);
        List<MovieMetadata> GetBulkMovieInfo(List<int> tmdbIds);

        HashSet<int> GetChangedMovies(DateTime startTime);
    }
}
