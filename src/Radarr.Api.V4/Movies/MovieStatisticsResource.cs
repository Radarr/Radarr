using NzbDrone.Core.MovieStats;

namespace Radarr.Api.V4.Movies;

public class MovieStatisticsResource
{
    public int MovieFileCount { get; set; }
    public long SizeOnDisk { get; set; }
    public List<string>? ReleaseGroups { get; set; }
}

public static class MovieStatisticsResourceMapper
{
    public static MovieStatisticsResource ToResource(this MovieStatistics model)
    {
        return new MovieStatisticsResource
        {
            MovieFileCount = model.MovieFileCount,
            SizeOnDisk = model.SizeOnDisk,
            ReleaseGroups = model.ReleaseGroups
        };
    }
}
