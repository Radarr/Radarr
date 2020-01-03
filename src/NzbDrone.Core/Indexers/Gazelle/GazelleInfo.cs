using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Gazelle
{
    public class GazelleInfo : TorrentInfo
    {
        public bool? Scene { get; set; }
    }
}
