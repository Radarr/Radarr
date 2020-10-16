using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Model
{
    public class ParsedMovieInfo
    {
        public ParsedMovieInfo()
        {
            MovieTitles = new List<string>();
            Languages = new List<Language>();
        }

        public List<string> MovieTitles { get; set; }
        public string OriginalTitle { get; set; }
        public string ReleaseTitle { get; set; }
        public string SimpleReleaseTitle { get; set; }
        public QualityModel Quality { get; set; }
        public List<Language> Languages { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }
        public string Edition { get; set; }
        public int Year { get; set; }
        public string ImdbId { get; set; }
        public int TmdbId { get; set; }
        [JsonIgnore]
        public Dictionary<string, object> ExtraInfo { get; set; } = new Dictionary<string, object>();

        public string PrimaryMovieTitle
        {
            get
            {
                if (MovieTitles.Count > 0)
                {
                    return MovieTitles[0];
                }

                return null;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} {2}", PrimaryMovieTitle, Year, Quality);
        }
    }
}
