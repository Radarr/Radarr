using System.Collections.Generic;
using Lidarr.Api.V1.Albums;
using Lidarr.Api.V1.Artist;
using Lidarr.Http.REST;
using NzbDrone.Core.Parser.Model;

namespace Lidarr.Api.V1.Parse
{
    public class ParseResource : RestResource
    {
        public string Title { get; set; }
        public ParsedAlbumInfo ParsedAlbumInfo { get; set; }
        public ArtistResource Artist { get; set; }
        public List<AlbumResource> Albums { get; set; }
    }
}
