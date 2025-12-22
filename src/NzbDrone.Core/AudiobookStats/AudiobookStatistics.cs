using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.AudiobookStats
{
    public class AudiobookStatistics : ResultSet
    {
        public int AudiobookId { get; set; }
        public int AudiobookFileCount { get; set; }
        public long SizeOnDisk { get; set; }
        public long TotalDurationSeconds { get; set; }
        public string ReleaseGroupsString { get; set; }

        public int TotalDurationMinutes => (int)(TotalDurationSeconds / 60);

        public List<string> ReleaseGroups
        {
            get
            {
                var releaseGroups = new List<string>();

                if (ReleaseGroupsString.IsNotNullOrWhiteSpace())
                {
                    releaseGroups = ReleaseGroupsString
                        .Split('|')
                        .Distinct()
                        .Where(rg => rg.IsNotNullOrWhiteSpace())
                        .OrderBy(rg => rg)
                        .ToList();
                }

                return releaseGroups;
            }
        }
    }
}
