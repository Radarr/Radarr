using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Api.Music;
using NzbDrone.Api.Albums;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Api.Parse
{
    public class ParseResource : RestResource
    {
        public string Title { get; set; }
        public ParsedAlbumInfo ParsedAlbumInfo { get; set; }
        public ArtistResource Artist { get; set; }
        public List<AlbumResource> Albums { get; set; }
    }
}