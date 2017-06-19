using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class ArtistInfoResource
    {
        public ArtistInfoResource() { }

        public List<string> Genres { get; set; }
        public string AristUrl { get; set; }
        public string Overview { get; set; }
        public string Id { get; set; }
        public List<ImageResource> Images { get; set; }
        public string ArtistName { get; set; }
    }
}
