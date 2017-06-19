using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class TrackResource
    {
        public TrackResource()
        {

        }

        public int DiscNumber { get; set; }
        public int DurationMs { get; set; }
        public string Href { get; set; }
        public string Id { get; set; }
        public string TrackName { get; set; }
        public int TrackNumber { get; set; }
        public bool Explicit { get; set; }
        public List<ArtistResource> Artists { get; set; }

    }
}
