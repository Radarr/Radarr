using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

public class TorrentFileResponse
{
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("components")]
    public List<string> Components { get; set; }
    [JsonProperty("length")]
    public long Length { get; set; }
    [JsonProperty("included")]
    public bool Included { get; set; }
}
