using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

public class RootResponse
{
    [JsonProperty("apis")]
    public Dictionary<string, string> Apis { get; set; }
    [JsonProperty("server")]
    public string Server { get; set; }
    [JsonProperty("version")]
    public string Version { get; set; }
}
