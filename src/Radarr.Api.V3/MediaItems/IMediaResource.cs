using System;
using System.Collections.Generic;

namespace Radarr.Api.V3.MediaItems
{
    public interface IMediaResource
    {
        int Id { get; set; }
        bool Monitored { get; set; }
        int QualityProfileId { get; set; }
        string Path { get; set; }
        string RootFolderPath { get; set; }
        DateTime Added { get; set; }
        HashSet<int> Tags { get; set; }
    }
}
