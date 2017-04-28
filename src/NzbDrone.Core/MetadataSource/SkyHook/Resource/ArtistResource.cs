using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class AlbumResource
    { 
        public AlbumResource()
        {

        }

        public string ArtistName { get; set; }
        public int ArtistId { get; set; }
        public string CollectionName { get; set; }
    }

    public class ArtistResource
    {
        public ArtistResource()
        {

        }

        public int ResultCount { get; set; }
        public List<AlbumResource> Results { get; set; }
        //public string ArtistName { get; set; }
        //public List<AlbumResource> Albums { get; set; }
    }
}
