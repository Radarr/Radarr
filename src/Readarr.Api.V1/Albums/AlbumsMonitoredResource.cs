using System.Collections.Generic;

namespace Readarr.Api.V1.Albums
{
    public class AlbumsMonitoredResource
    {
        public List<int> BookIds { get; set; }
        public bool Monitored { get; set; }
    }
}
