using System.Collections.Generic;
using NzbDrone.Core.Music;

namespace NzbDrone.Api.AlbumPass
{
    public class AlbumStudioResource
    {
        public List<Core.Music.Artist> Artist { get; set; }
        public MonitoringOptions MonitoringOptions { get; set; }
    }
}
