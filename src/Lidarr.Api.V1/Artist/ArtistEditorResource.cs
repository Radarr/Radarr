using System.Collections.Generic;
using NzbDrone.Core.Music;

namespace Lidarr.Api.V1.Artist
{
    public class ArtistEditorResource
    {
        public List<int> ArtistIds { get; set; }
        public bool? Monitored { get; set; }
        public int? QualityProfileId { get; set; }
        public int? MetadataProfileId { get; set; }
        public bool? AlbumFolder { get; set; }
        public string RootFolderPath { get; set; }
        public List<int> Tags { get; set; }
        public ApplyTags ApplyTags { get; set; }
        public bool MoveFiles { get; set; }
    }

    public enum ApplyTags
    {
        Add,
        Remove,
        Replace
    }
}
