using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

public class TorrentResponse
{
    public string info_hash { get; set; }
    public string name { get; set; }
    public List<TorrentFileResponse> files { get; set; }
}
