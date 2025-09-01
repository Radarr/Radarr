namespace NzbDrone.Core.Download.Clients.RQBit
{
    public class RQBitTorrent
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Hash { get; set; }
        public long TotalSize { get; set; }
        public long RemainingSize { get; set; }
        public string Category { get; set; }
        public double? Ratio { get; set; }
        public long DownRate { get; set; }
        public bool IsFinished { get; set; }
        public bool IsActive { get; set; }
        public bool HasError { get; set; }
        public long FinishedTime { get; set; }
        public string Path { get; set; }
        public string Message { get; set; }
    }
}
