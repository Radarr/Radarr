using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.MovieStats
{
    public class MovieStatistics : ResultSet
    {
        public int MovieId { get; set; }
        public int MovieFileCount { get; set; }
        public long SizeOnDisk { get; set; }
        public string ReleaseGroupsString { get; set; }

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
