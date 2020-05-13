using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.AuthorStats
{
    public class AuthorStatistics : ResultSet
    {
        public int AuthorId { get; set; }
        public int BookCount { get; set; }
        public int BookFileCount { get; set; }
        public long SizeOnDisk { get; set; }
        public List<BookStatistics> BookStatistics { get; set; }
    }
}
