using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

public class PostTorrentResponse
{
    [JsonProperty("id")]
    public long Id { get; set; }
    [JsonProperty("details")]
    public PostTorrentDetailsResponse Details { get; set; }
    [JsonProperty("output_folder")]
    public string OutputFolder { get; set; }
    [JsonProperty("seen_peers")]
    public List<string> SeenPeers { get; set; }
}

public class PostTorrentDetailsResponse
{
    [JsonProperty("info_hash")]
    public string InfoHash { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("files")]
    public List<TorrentFileResponse> Files { get; set; }
}
