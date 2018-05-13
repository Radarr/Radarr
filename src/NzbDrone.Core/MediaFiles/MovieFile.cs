using System;
using System.Collections.Generic;
using Marr.Data;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Movies;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.MediaFiles
{
    public class MovieFile : ModelBase
    {
        public int MovieId { get; set; }
        public string RelativePath { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string SceneName { get; set; }
        public string ReleaseGroup { get; set; }
        public QualityModel Quality { get; set; }
        public MediaInfoModel MediaInfo { get; set; }
        public string Edition { get; set; }
        public LazyLoaded<Movie> Movie { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", Id, RelativePath);
        }
    }
}
