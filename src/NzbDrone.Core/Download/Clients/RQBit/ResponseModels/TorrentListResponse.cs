using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

public class TorrentListResponse
{
    [JsonProperty("torrents")]
    public List<TorrentListingResponse> torrents { get; set; }
}
