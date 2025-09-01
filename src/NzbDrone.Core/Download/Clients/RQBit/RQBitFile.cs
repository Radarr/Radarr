namespace NzbDrone.Core.Download.Clients.RQBit;

public class RQBitFile
{
    public string FileName { get; set; }
    public int FileSize { get; set; }
    public int FileDownloaded { get; set; }
}
