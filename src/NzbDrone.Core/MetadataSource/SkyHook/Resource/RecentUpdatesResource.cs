using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class RecentUpdatesResource
    {
        public int Count { get; set; }
        public bool Limited { get; set; }
        public DateTime Since { get; set; }
        public List<string> Items { get; set; }
    }
}
