using System;
using System.Collections.Generic;
using NzbDrone.Core.Movies;

namespace Radarr.Api.V3.Collections
{
    public class CollectionUpdateResource
    {
        public List<int> CollectionIds { get; set; }
        public bool? Monitored { get; set; }
        public bool? MonitorMovies { get; set; }
        public int? QualityProfileId { get; set; }
        public string RootFolderPath { get; set; }
        public MovieStatusType? MinimumAvailability { get; set; }
    }
}
