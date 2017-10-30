using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;
using Lidarr.Api.V3.Albums;
using Lidarr.Api.V3.Artist;
using Lidarr.Http.REST;

namespace Lidarr.Api.V3.Parse
{
    public class ParseResource : RestResource
    {
        public string Title { get; set; }
        public ParsedTrackInfo ParsedAlbumInfo { get; set; }
        public ArtistResource Artist { get; set; }
        public List<AlbumResource> Albums { get; set; }
    }
}
