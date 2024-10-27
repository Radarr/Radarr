namespace NzbDrone.Core.Download.Clients.RQBit.ResponseModels;

public class TorrentListingResponse
{
    public long id { get; set; }
    public string info_hash { get; set; }
}
