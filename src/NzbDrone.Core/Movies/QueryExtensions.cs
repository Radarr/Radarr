using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Movies
{
    public static class EnumerableExtensions
    {
        public static Movie FirstWithYear(this IEnumerable<Movie> query, int? year)
        {
            return year.HasValue ? query.FirstOrDefault(movie => movie.Year == year || movie.MovieMetadata.Value.SecondaryYear == year) : query.FirstOrDefault();
        }
    }
}
