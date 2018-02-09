using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Model
{
    public class ParsedMovieInfo
    {
        public string MovieTitle { get; set; }
        public string SimpleTitle { get; set; }
        public SeriesTitleInfo MovieTitleInfo { get; set; }
        public QualityModel Quality { get; set; }
        //public int SeasonNumber { get; set; }
        public List<Language> Languages { get; set; }
        //public bool FullSeason { get; set; }
        //public bool Special { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }
        public string Edition { get; set;}
        public int Year { get; set; }
        public string ImdbId { get; set; }

        public ParsedMovieInfo()
        {

        }

        public override string ToString()
        {
            return string.Format("{0} - {1} {2}", MovieTitle, Year, Quality);
        }
    }
}