using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

public class TorrentFileResponse
{
    public string Name { get; set; }
    public List<string> Components { get; set; }
    public long Length { get; set; }
    public bool Included { get; set; }
}
