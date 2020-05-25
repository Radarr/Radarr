using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.AuthorStats
{
    public class BookStatistics : ResultSet
    {
        public int AuthorId { get; set; }
        public int BookId { get; set; }
        public int BookFileCount { get; set; }
        public int BookCount { get; set; }
        public long SizeOnDisk { get; set; }
        public int TotalBookCount { get; set; }
    }
}
