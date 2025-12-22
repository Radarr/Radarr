using System.Collections.Generic;
using NzbDrone.Core.AudiobookStats;

namespace Radarr.Api.V3.Audiobooks
{
    public class AudiobookStatisticsResource
    {
        public int AudiobookFileCount { get; set; }
        public long SizeOnDisk { get; set; }
        public int TotalDurationMinutes { get; set; }
        public List<string> ReleaseGroups { get; set; }
    }

    public static class AudiobookStatisticsResourceMapper
    {
        public static AudiobookStatisticsResource ToResource(this AudiobookStatistics model)
        {
            if (model == null)
            {
                return null;
            }

            return new AudiobookStatisticsResource
            {
                AudiobookFileCount = model.AudiobookFileCount,
                SizeOnDisk = model.SizeOnDisk,
                TotalDurationMinutes = model.TotalDurationMinutes,
                ReleaseGroups = model.ReleaseGroups
            };
        }
    }
}
