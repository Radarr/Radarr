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
        public int CollectionId { get; set; }
        public string PrimaryGenreName { get; set; }
        public string ArtworkUrl100 { get; set; }
        public string Country { get; set; }
        public string CollectionExplicitness { get; set; }
        public int TrackCount { get; set; }
        public string Copyright { get; set; }
        public DateTime ReleaseDate { get; set; }

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
