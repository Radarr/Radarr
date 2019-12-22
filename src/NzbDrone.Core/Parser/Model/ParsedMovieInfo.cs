using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Languages;

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
        /// <summary>
        /// Extra info is a dictionary containing extra info needed for correct quality assignement.
        /// It is expanded by the augmenters.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, object> ExtraInfo = new Dictionary<string, object>();
        public List<Language> Languages = new List<Language>();
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }
        public string Edition { get; set;}
        public int Year { get; set; }
        public string ImdbId { get; set; }

        public override string ToString()
        {
            return String.Format("{0} - {1} {2}", MovieTitle, Year, Quality);
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
