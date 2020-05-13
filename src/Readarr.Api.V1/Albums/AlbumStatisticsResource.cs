using NzbDrone.Core.AuthorStats;

namespace Readarr.Api.V1.Albums
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
                if (TrackCount == 0)
                {
                    return 0;
                }

                return TrackFileCount / (decimal)TrackCount * 100;
            }
        }
    }

    public static class AlbumStatisticsResourceMapper
    {
        public static AlbumStatisticsResource ToResource(this BookStatistics model)
        {
            if (model == null)
            {
                return null;
            }

            return new AlbumStatisticsResource
            {
                TrackFileCount = model.BookFileCount,
                TrackCount = model.BookCount,
                SizeOnDisk = model.SizeOnDisk
            };
        }
    }
}
