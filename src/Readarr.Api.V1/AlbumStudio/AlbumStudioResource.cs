using System.Collections.Generic;
using NzbDrone.Core.Books;

namespace Readarr.Api.V1.AlbumStudio
{
    public class AlbumStudioResource
    {
        public List<AlbumStudioArtistResource> Artist { get; set; }
        public MonitoringOptions MonitoringOptions { get; set; }
    }
}
