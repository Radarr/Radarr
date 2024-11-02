using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

public class TorrentResponse
{
    [JsonProperty("info_hash")]
    public string InfoHash { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("files")]
    public List<TorrentFileResponse> Files { get; set; }
    [JsonProperty("output_folder")]
    public string  OutputFolder { get; set; }
}
