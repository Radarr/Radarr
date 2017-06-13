using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class ArtistResource
    {
        public ArtistResource() {
            Albums = new List<AlbumResource>();
            Tracks = new List<TrackResource>();
        }

        public List<string> Genres { get; set; }
        public string AristUrl { get; set; }
        public string Overview { get; set; }
        public string Id { get; set; }
        public List<ImageResource> Images { get; set; }
        public string ArtistName { get; set; }
        public List<AlbumResource> Albums { get; set; }
        public List<TrackResource> Tracks { get; set; }
    }
}
