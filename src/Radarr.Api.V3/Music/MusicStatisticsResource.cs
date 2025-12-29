using System.Collections.Generic;
using NzbDrone.Core.MusicStats;

namespace Radarr.Api.V3.Music
{
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S6968", Justification = "Follows existing resource patterns")]
    public class MusicStatisticsResource
    {
        public int TrackCount { get; set; }
        public int TrackFileCount { get; set; }
        public long SizeOnDisk { get; set; }
        public List<string> ReleaseGroups { get; set; }

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

    public static class MusicStatisticsResourceMapper
    {
        public static MusicStatisticsResource ToResource(this MusicStatistics model)
        {
            if (model == null)
            {
                return null;
            }

            return new MusicStatisticsResource
            {
                TrackCount = model.TrackCount,
                TrackFileCount = model.TrackFileCount,
                SizeOnDisk = model.SizeOnDisk,
                ReleaseGroups = model.ReleaseGroups
            };
        }
    }
}
