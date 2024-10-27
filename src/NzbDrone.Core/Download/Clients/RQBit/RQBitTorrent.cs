namespace NzbDrone.Core.Download.Clients.RQBit;

public class RQBitTorrent
{
    public long id { get; set; }
    public string Name { get; set; }
    public string Hash { get; set; }
    public long TotalSize { get; set; }
    public long RemainingSize { get; set; }
    public string Category { get; set; }
    public double? Ratio { get; set; }
    public long DownRate { get; set; }
    public bool IsFinished { get; set; }
    public bool IsActive { get; set; }
    public long FinishedTime { get; set; }
}
