using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class ReleaseResource
    {
        public string Disambiguation { get; set; }
        public List<string> Country { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string Id { get; set; }
        public List<string> OldIds { get; set; }
        public List<string> Label { get; set; }
        public List<MediumResource> Media { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public int TrackCount { get; set; }
        public List<TrackResource> Tracks { get; set; }
    }
}
