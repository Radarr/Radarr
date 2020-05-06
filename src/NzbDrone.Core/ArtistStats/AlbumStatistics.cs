using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.ArtistStats
{
    public class AlbumStatistics : ResultSet
    {
        public int AuthorId { get; set; }
        public int BookId { get; set; }
        public int TrackFileCount { get; set; }
        public int TrackCount { get; set; }
        public int AvailableTrackCount { get; set; }
        public int TotalTrackCount { get; set; }
        public long SizeOnDisk { get; set; }
    }
}
