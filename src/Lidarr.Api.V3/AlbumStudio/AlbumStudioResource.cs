using System.Collections.Generic;
using NzbDrone.Core.Music;

namespace Lidarr.Api.V3.AlbumStudio
{
    public class AlbumStudioResource
    {
        public List<AlbumStudioArtistResource> Artist { get; set; }
        public MonitoringOptions MonitoringOptions { get; set; }
    }
}
