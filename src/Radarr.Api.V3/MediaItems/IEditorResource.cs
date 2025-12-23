using System.Collections.Generic;

namespace Radarr.Api.V3.MediaItems
{
    public interface IEditorResource
    {
        List<int> Ids { get; }
        bool? Monitored { get; set; }
        int? QualityProfileId { get; set; }
        string RootFolderPath { get; set; }
        List<int> Tags { get; set; }
        ApplyTags ApplyTags { get; set; }
        bool DeleteFiles { get; set; }
    }
}
