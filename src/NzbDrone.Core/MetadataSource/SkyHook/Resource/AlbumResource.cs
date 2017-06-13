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
        //public string AlbumType { get; set; } // Might need to make this a separate class
        public List<ArtistInfoResource> Artists { get; set; } // Will always be length of 1 unless a compilation
        public string Url { get; set; } // Link to the endpoint api to give full info for this object
        public string Id { get; set; } // This is a unique Album ID. Needed for all future API calls
        public int Year { get; set; }
        public List<ImageResource> Images { get; set; }
        public string AlbumName { get; set; } // In case of a takedown, this may be empty
        public string Overview { get; set; }
        public List<string> Genres { get; set; }
        public string Label { get; set; }
    }

    
}
