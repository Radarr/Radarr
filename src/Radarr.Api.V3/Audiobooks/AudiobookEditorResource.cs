using System.Collections.Generic;
using Radarr.Api.V3.MediaItems;

namespace Radarr.Api.V3.Audiobooks
{
    public class AudiobookEditorResource : IEditorResource
    {
        public List<int> AudiobookIds { get; set; }
        public bool? Monitored { get; set; }
        public int? QualityProfileId { get; set; }
        public string RootFolderPath { get; set; }
        public List<int> Tags { get; set; }
        public ApplyTags ApplyTags { get; set; }
        public bool MoveFiles { get; set; }
        public bool DeleteFiles { get; set; }

        List<int> IEditorResource.Ids => AudiobookIds;
    }
}
