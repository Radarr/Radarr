using System;
using NzbDrone.Core.ArtistStats;

namespace Lidarr.Api.V1.Albums
{
    public class AlbumStatisticsResource
    {
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

    public static class AlbumStatisticsResourceMapper
    {
        public static AlbumStatisticsResource ToResource(this AlbumStatistics model)
        {
            if (model == null) return null;

            return new AlbumStatisticsResource
            {
                TrackFileCount = model.TrackFileCount,
                TrackCount = model.TrackCount,
                TotalTrackCount = model.TotalTrackCount,
                SizeOnDisk = model.SizeOnDisk
            };
        }
    }
}
