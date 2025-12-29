using NzbDrone.Core.Analytics;

namespace Radarr.Api.V3.Dashboard
{
    public class DashboardResource
    {
        public MediaTypeStatisticsResource Movies { get; set; }
        public MediaTypeStatisticsResource Books { get; set; }
        public MediaTypeStatisticsResource Audiobooks { get; set; }
        public long TotalSizeOnDisk { get; set; }
    }

    public class MediaTypeStatisticsResource
    {
        public int Total { get; set; }
        public int WithFiles { get; set; }
        public int Missing { get; set; }
        public int Monitored { get; set; }
        public int Unmonitored { get; set; }
        public long SizeOnDisk { get; set; }
        public int TotalDurationMinutes { get; set; }
    }

    public static class DashboardResourceMapper
    {
        public static DashboardResource ToResource(this DashboardStatistics model)
        {
            if (model == null)
            {
                return null;
            }

            return new DashboardResource
            {
                Movies = model.Movies.ToResource(),
                Books = model.Books.ToResource(),
                Audiobooks = model.Audiobooks.ToResource(),
                TotalSizeOnDisk = model.TotalSizeOnDisk
            };
        }

        public static MediaTypeStatisticsResource ToResource(this MediaTypeStatistics model)
        {
            if (model == null)
            {
                return null;
            }

            return new MediaTypeStatisticsResource
            {
                Total = model.Total,
                WithFiles = model.WithFiles,
                Missing = model.Missing,
                Monitored = model.Monitored,
                Unmonitored = model.Unmonitored,
                SizeOnDisk = model.SizeOnDisk,
                TotalDurationMinutes = model.TotalDurationMinutes
            };
        }
    }
}
