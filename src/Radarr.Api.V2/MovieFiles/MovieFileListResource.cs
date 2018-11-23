using System.Collections.Generic;
using NzbDrone.Core.Qualities;

namespace Radarr.Api.V2.MovieFiles
{
    public class MovieFileListResource
    {
        public List<int> MovieFileIds { get; set; }
        public QualityModel Quality { get; set; }
    }
}
