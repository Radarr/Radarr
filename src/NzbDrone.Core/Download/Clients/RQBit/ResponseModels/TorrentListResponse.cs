using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

public class TorrentListResponse
{
    public List<TorrentListingResponse> torrents { get; set; }
}
