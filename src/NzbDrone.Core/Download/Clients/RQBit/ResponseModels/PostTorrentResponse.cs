using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

public class PostTorrentResponse
{
    public long id { get; set; }
    public PostTorrentDetailsResponse details { get; set; }
    public string output_folder { get; set; }
    public List<string> seen_peers { get; set; }
}

public class PostTorrentDetailsResponse
{
    public string info_hash { get; set; }
    public string name { get; set; }
    public List<TorrentFileResponse> files { get; set; }
}
