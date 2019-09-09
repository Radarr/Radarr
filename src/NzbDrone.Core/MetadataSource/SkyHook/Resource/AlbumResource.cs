using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class AlbumResource
    {
        public string ArtistId { get; set; }
        public List<ArtistResource> Artists { get; set; }
        public string Disambiguation { get; set; }
        public string Overview { get; set; }
        public string Id { get; set; }
        public List<string> OldIds { get; set; }
        public List<ImageResource> Images { get; set; }
        public List<LinkResource> Links { get; set; }
        public List<string> Genres { get; set; }
        public RatingResource Rating { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<ReleaseResource> Releases { get; set; }
        public List<string> SecondaryTypes { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public List<string> ReleaseStatuses { get; set; }
    }
}
