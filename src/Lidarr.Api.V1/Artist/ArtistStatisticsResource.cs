using System;
using NzbDrone.Core.ArtistStats;

namespace Lidarr.Api.V1.Artist
{
    public class ArtistStatisticsResource
    {
        public int AlbumCount { get; set; }
        public int TrackFileCount { get; set; }
        public int TrackCount { get; set; }
        public int TotalTrackCount { get; set; }
        public long SizeOnDisk { get; set; }

        public decimal PercentOfTracks
        {
            get
            {
                if (TrackCount == 0) return 0;

                return (decimal)TrackFileCount / (decimal)TrackCount * 100;
            }
        }
    }

    public static class ArtistStatisticsResourceMapper
    {
        public static ArtistStatisticsResource ToResource(this ArtistStatistics model)
        {
            if (model == null) return null;

            return new ArtistStatisticsResource
            {
                AlbumCount = model.AlbumCount,
                TrackFileCount = model.TrackFileCount,
                TrackCount = model.TrackCount,
                TotalTrackCount = model.TotalTrackCount,
                SizeOnDisk = model.SizeOnDisk
            };
        }
    }
}
