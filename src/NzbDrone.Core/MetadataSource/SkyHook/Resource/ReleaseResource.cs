using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class ReleaseResource
    {
        public string Id { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int MediaCount { get; set; }
        public int TrackCount { get; set; }
        public string Disambiguation { get; set; }
        public List<string> Label {get; set;}
        public List<string> Country { get; set; }
        public string Title { get; set; }
        public string Format { get; set; }
    }
}
