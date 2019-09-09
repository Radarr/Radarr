using NzbDrone.Core.Parser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Indexers.Gazelle
{
    public class GazelleInfo : TorrentInfo
    {
        public bool? Scene { get; set; }
    }
}
