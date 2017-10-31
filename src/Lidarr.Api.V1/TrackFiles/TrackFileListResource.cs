using System.Collections.Generic;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;

namespace Lidarr.Api.V1.TrackFiles
{
    public class TrackFileListResource
    {
        public List<int> TrackFileIds { get; set; }
        public Language Language { get; set; }
        public QualityModel Quality { get; set; }
    }
}
