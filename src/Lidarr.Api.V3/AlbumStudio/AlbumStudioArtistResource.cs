using System.Collections.Generic;
using Lidarr.Api.V3.Albums;

namespace Lidarr.Api.V3.AlbumStudio
{
    public class AlbumStudioArtistResource
    {
        public int Id { get; set; }
        public bool? Monitored { get; set; }
        public List<AlbumResource> Albums { get; set; }
    }
}
