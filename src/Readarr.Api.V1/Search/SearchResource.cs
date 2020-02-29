using Readarr.Api.V1.Albums;
using Readarr.Api.V1.Artist;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Search
{
    public class
    SearchResource : RestResource
    {
        public string ForeignId { get; set; }
        public ArtistResource Artist { get; set; }
        public AlbumResource Album { get; set; }
    }
}
