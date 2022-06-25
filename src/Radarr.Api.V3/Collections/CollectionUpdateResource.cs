using System;
using System.Collections.Generic;

namespace Radarr.Api.V3.Collections
{
    public class CollectionUpdateResource
    {
        public List<int> CollectionIds { get; set; }
        public bool? Monitored { get; set; }
        public bool? MonitorMovies { get; set; }
    }
}
