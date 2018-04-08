using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Model
{
    /// <summary>
    /// Object containing all info our intelligent parser could find out from release / file title, release info and media info.
    /// </summary>
    public class ParsedMovieInfo
    {
        /// <summary>
        /// The fully Parsed title. This is useful for finding the matching movie in the database.
        /// </summary>
        public string MovieTitle { get; set; }
        /// <summary>
        /// The simple release title replaces the actual movie title parsed with A Movie in the release / file title.
        /// This is useful to not accidentaly identify stuff inside the actual movie title as quality tags, etc.
        /// It also removes unecessary stuff such as file extensions.
        /// </summary>
        public string SimpleReleaseTitle { get; set; }
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
