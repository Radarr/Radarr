using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Datastore.Extensions;
using Marr.Data.QGen;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.RomanNumerals;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Movies;
using CoreParser = NzbDrone.Core.Parser.Parser;
namespace NzbDrone.Core
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
