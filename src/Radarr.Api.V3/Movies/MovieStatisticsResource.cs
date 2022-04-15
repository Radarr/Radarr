using System.Collections.Generic;
using NzbDrone.Core.MovieStats;

namespace Radarr.Api.V3.Movies
{
    public class MovieStatisticsResource
    {
        public int MovieFileCount { get; set; }
        public long SizeOnDisk { get; set; }
        public List<string> ReleaseGroups { get; set; }
    }

    public static class SeriesStatisticsResourceMapper
    {
        public static MovieStatisticsResource ToResource(this MovieStatistics model)
        {
            if (model == null)
            {
                return null;
            }

            return new MovieStatisticsResource
            {
                MovieFileCount = model.MovieFileCount,
                SizeOnDisk = model.SizeOnDisk,
                ReleaseGroups = model.ReleaseGroups
            };
        }
    }
}
