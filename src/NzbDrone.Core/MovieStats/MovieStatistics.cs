using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.MovieStats
{
    public class MovieStatistics : ResultSet
    {
        public int MovieId { get; set; }
        public int MovieFileCount { get; set; }
        public long SizeOnDisk { get; set; }
        public List<string> ReleaseGroups { get; set; }
    }
}
