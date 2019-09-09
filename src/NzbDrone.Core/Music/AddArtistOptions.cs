using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{
    public class AddArtistOptions : MonitoringOptions
    {
        public bool SearchForMissingAlbums { get; set; }
    }
}
