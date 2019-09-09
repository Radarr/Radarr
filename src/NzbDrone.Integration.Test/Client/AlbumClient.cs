using System.Collections.Generic;
using Lidarr.Api.V1.Albums;
using RestSharp;

namespace NzbDrone.Integration.Test.Client
{
    public class AlbumClient : ClientBase<AlbumResource>
    {
        public AlbumClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey, "album")
        {
        }

        public List<AlbumResource> GetAlbumsInArtist(int artistId)
        {
            var request = BuildRequest("?artistId=" + artistId.ToString());
            return Get<List<AlbumResource>>(request);
        }
    }
}
