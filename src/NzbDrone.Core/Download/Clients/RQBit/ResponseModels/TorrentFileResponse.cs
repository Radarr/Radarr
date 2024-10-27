using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

public class TorrentFileResponse
{
    public string name { get; set; }
    public List<string> components { get; set; }
    public long length { get; set; }
    public bool included { get; set; }
}
