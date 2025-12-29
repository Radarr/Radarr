namespace NzbDrone.Core.Analytics
{
    public class DashboardStatistics
    {
        public MediaTypeStatistics Movies { get; set; }
        public MediaTypeStatistics Books { get; set; }
        public MediaTypeStatistics Audiobooks { get; set; }
        public long TotalSizeOnDisk { get; set; }
    }

    public class MediaTypeStatistics
    {
        public int Total { get; set; }
        public int WithFiles { get; set; }
        public int Missing { get; set; }
        public int Monitored { get; set; }
        public int Unmonitored { get; set; }
        public long SizeOnDisk { get; set; }
        public int TotalDurationMinutes { get; set; }
    }
}
