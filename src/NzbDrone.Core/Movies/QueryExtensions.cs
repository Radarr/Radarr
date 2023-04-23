using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Movies
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<Movie> AllWithYear(this IEnumerable<Movie> query, int? year)
        {
            return year.HasValue ? query.Where(movie => movie.Year == year || movie.MovieMetadata.Value.SecondaryYear == year) : query;
        }
    }
}
