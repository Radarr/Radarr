using System.Collections.Generic;
using System.Linq;
using Marr.Data.QGen;

namespace NzbDrone.Core.Movies
{
    public static class QueryExtensions
    {
        public static Movie FirstWithYear(this SortBuilder<Movie> query, int? year)
        {
            return year.HasValue ? query.FirstOrDefault(movie => movie.Year == year || movie.SecondaryYear == year) : query.FirstOrDefault();
        }
    }

    public static class EnumerableExtensions
    {
        public static Movie FirstWithYear(this IEnumerable<Movie> query, int? year)
        {
            return year.HasValue ? query.FirstOrDefault(movie => movie.Year == year || movie.SecondaryYear == year) : query.FirstOrDefault();
        }
    }
}
