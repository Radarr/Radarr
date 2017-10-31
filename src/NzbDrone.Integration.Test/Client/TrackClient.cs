using System.Collections.Generic;
using Lidarr.Api.V1.Tracks;
using RestSharp;

namespace NzbDrone.Integration.Test.Client
{
    public class TrackClient : ClientBase<TrackResource>
    {
        public TrackClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey, "track")
        {
        }

        public List<TrackResource> GetTracksInArtist(int artistId)
        {
            var request = BuildRequest("?artistId=" + artistId.ToString());
            return Get<List<TrackResource>>(request);
        }
    }
}
