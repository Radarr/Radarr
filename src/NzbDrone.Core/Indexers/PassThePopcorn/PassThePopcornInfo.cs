using NzbDrone.Core.Parser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Indexers.PassThePopcorn
{
    public class PassThePopcornInfo : TorrentInfo
    {
        public bool? Golden { get; set; }
        public bool? Scene { get; set; }
        public bool? Approved { get; set; }
    }
}
