using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.HDBits
{
    public class HDBitsInfo : TorrentInfo
    {
        public bool? Internal { get; set; }
    }
}
