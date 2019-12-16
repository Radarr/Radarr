using Lidarr.Http.REST;
using Lidarr.Api.V1.Artist;
using Lidarr.Api.V1.Albums;

namespace Lidarr.Api.V1.Search
{
    public class
    SearchResource : RestResource
    {
        public string ForeignId { get; set; }
        public ArtistResource Artist { get; set; }
        public AlbumResource Album { get; set; }
    }
}
