using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators
{
    public class AggregateLanguage : IAggregateLocalMovie
    {
        private readonly Logger _logger;

        public AggregateLanguage(Logger logger)
        {
            _logger = logger;
        }

        public LocalMovie Aggregate(LocalMovie localMovie, bool otherFiles)
        {
            // Get languages in preferred order, download client item, folder and finally file.
            // Non-English languages will be preferred later, in the event there is a conflict
            // between parsed languages the more preferred item will be used.

            var languages = new List<Language>();

            languages.AddRange(GetLanguage(localMovie.DownloadClientMovieInfo));
            languages.AddRange(GetLanguage(localMovie.FolderMovieInfo));
            languages.AddRange(GetLanguage(localMovie.FileMovieInfo));

            var language = new List<Language> { languages.FirstOrDefault(l => l != Language.English) ?? Language.English };

            _logger.Debug("Using language: {0}", language.First());

            localMovie.Languages = language;

            return localMovie;
        }

        private List<Language> GetLanguage(ParsedMovieInfo parsedMovieInfo)
        {
            if (parsedMovieInfo == null)
            {
                // English is the default language when otherwise unknown

                return new List<Language> { Language.English };
            }

            return parsedMovieInfo.Languages;
        }
    }
}
