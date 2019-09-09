using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class TrackResource
    {
        public TrackResource()
        {

        }

        public string ArtistId { get; set; }
        public int DurationMs { get; set; }
        public string Id { get; set; }
        public List<string> OldIds { get; set; }
        public string RecordingId { get; set; }
        public List<string> OldRecordingIds { get; set; }
        public string TrackName { get; set; }
        public string TrackNumber { get; set; }
        public int TrackPosition { get; set; }
        public bool Explicit { get; set; }
        public int MediumNumber { get; set; }

    }
}
