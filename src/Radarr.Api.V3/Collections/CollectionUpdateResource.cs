using System;
using System.Collections.Generic;

namespace Radarr.Api.V3.Collections
{
    public class CollectionUpdateResource
    {
        public List<CollectionUpdateCollectionResource> Collections { get; set; }
        public bool? MonitorMovies { get; set; }
    }
}
