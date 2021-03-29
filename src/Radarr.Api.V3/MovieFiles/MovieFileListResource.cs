using System.Collections.Generic;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace Radarr.Api.V3.MovieFiles
{
    public class MovieFileListResource
    {
        public List<int> MovieFileIds { get; set; }
        public List<Language> Languages { get; set; }
        public QualityModel Quality { get; set; }
        public string Edition { get; set; }
        public string ReleaseGroup { get; set; }
        public string SceneName { get; set; }
        public int? IndexerFlags { get; set; }
    }
}
