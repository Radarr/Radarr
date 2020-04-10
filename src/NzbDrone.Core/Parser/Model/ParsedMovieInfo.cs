using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Model
{
    public class ParsedMovieInfo
    {
        public string MovieTitle { get; set; }
        public string OriginalTitle { get; set; }
        public string ReleaseTitle { get; set; }
        public string SimpleReleaseTitle { get; set; }
        public QualityModel Quality { get; set; }
        public List<Language> Languages { get; set; } = new List<Language>();
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }
        public string Edition { get; set; }
        public int Year { get; set; }
        public string ImdbId { get; set; }
        [JsonIgnore]
        public Dictionary<string, object> ExtraInfo { get; set; } = new Dictionary<string, object>();

        public override string ToString()
        {
            return string.Format("{0} - {1} {2}", MovieTitle, Year, Quality);
        }

#if LIBRARY
        public static ParsedMovieInfo ParseMovieInfo(string title)
        {
            var parsedMovie = Parser.ParseMovieTitle(title, false);

            if (parsedMovie == null) return null;

            parsedMovie.Languages = LanguageParser.ParseLanguages(parsedMovie.SimpleReleaseTitle);

            parsedMovie.Quality = QualityParser.ParseQuality(parsedMovie.SimpleReleaseTitle);

            if (parsedMovie.Edition.IsNullOrWhiteSpace())
            {
                parsedMovie.Edition = Parser.ParseEdition(parsedMovie.SimpleReleaseTitle);
            }

            parsedMovie.ReleaseGroup = Parser.ParseReleaseGroup(parsedMovie.SimpleReleaseTitle);

            parsedMovie.ImdbId = Parser.ParseImdbId(parsedMovie.SimpleReleaseTitle);

            parsedMovie.Languages =
                LanguageParser.EnhanceLanguages(parsedMovie.SimpleReleaseTitle, parsedMovie.Languages);

            parsedMovie.Quality.Quality = Qualities.Quality.FindByInfo(parsedMovie.Quality.Source, parsedMovie.Quality.Resolution,
                parsedMovie.Quality.Modifier);

            return parsedMovie;
        }
#endif
    }
}
