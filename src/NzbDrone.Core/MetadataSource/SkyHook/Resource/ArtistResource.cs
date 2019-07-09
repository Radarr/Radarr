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
        }

        public List<string> Genres { get; set; }
        public string AristUrl { get; set; }
        public string Overview { get; set; }
        public string Type { get; set; }
        public string Disambiguation { get; set; }
        public string Id { get; set; }
        public List<string> OldIds { get; set; }
        public List<ImageResource> Images { get; set; }
        public List<LinkResource> Links { get; set; }
        public string ArtistName { get; set; }
        public List<string> ArtistAliases { get; set; }
        public List<AlbumResource> Albums { get; set; }
        public string Status { get; set; }
        public RatingResource Rating { get; set; }

    }
}
