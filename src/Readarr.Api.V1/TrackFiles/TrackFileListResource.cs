using System.Collections.Generic;
using NzbDrone.Core.Qualities;

namespace Readarr.Api.V1.TrackFiles
{
    public class TrackFileListResource
    {
        public List<int> TrackFileIds { get; set; }
        public QualityModel Quality { get; set; }
    }
}
