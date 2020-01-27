using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.PassThePopcorn
{
    public class PassThePopcornInfo : TorrentInfo
    {
        public bool? Golden { get; set; }
        public bool? Scene { get; set; }
        public bool? Approved { get; set; }
    }
}
