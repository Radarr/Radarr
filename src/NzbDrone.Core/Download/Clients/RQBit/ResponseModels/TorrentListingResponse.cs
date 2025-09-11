using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

public class TorrentListingResponse
{
    [JsonProperty("id")]
    public long Id { get; set; }
    [JsonProperty("info_hash")]
    public string InfoHash { get; set; }
}
