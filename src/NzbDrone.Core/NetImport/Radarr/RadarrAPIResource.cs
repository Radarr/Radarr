using System;
using System.Collections.Generic;

namespace NzbDrone.Core.NetImport.Radarr
{
    public class RadarrMovie
    {
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public int TmdbId { get; set; }
        public string Overview { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public bool Monitored { get; set; }
        public DateTime InCinemas { get; set; }
        public DateTime PhysicalRelease { get; set; }
        public int Year { get; set; }
        public string TitleSlug { get; set; }
    }
}
