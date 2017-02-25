using NzbDrone.Core.Parser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Indexers.HDBits
{
    public class HDBitsInfo : TorrentInfo
    {
        public bool? Internal { get; set; }
    }
}
