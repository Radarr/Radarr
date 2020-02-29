using System.Collections.Generic;

namespace Readarr.Api.V1.Albums
{
    public class AlbumsMonitoredResource
    {
        public List<int> AlbumIds { get; set; }
        public bool Monitored { get; set; }
    }
}
