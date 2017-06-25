using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.ArtistStats
{
    public class ArtistStatistics : ResultSet
    {
        public int ArtistId { get; set; }
        public int AlbumCount { get; set; }
        public int TrackFileCount { get; set; }
        public int TrackCount { get; set; }
        public int TotalTrackCount { get; set; }
        public long SizeOnDisk { get; set; }
        public List<AlbumStatistics> AlbumStatistics { get; set; }
    }
}
