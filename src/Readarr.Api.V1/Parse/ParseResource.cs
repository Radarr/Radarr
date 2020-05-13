using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;
using Readarr.Api.V1.Albums;
using Readarr.Api.V1.Artist;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Parse
{
    public class ParseResource : RestResource
    {
        public string Title { get; set; }
        public ParsedBookInfo ParsedAlbumInfo { get; set; }
        public ArtistResource Artist { get; set; }
        public List<AlbumResource> Albums { get; set; }
    }
}
