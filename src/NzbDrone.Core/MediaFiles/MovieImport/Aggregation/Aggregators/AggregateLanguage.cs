using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
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

            languages.AddRange(localMovie.DownloadClientMovieInfo?.Languages ?? new List<Language>());

            if (!languages.Any(l => l != Language.English))
            {
                languages = localMovie.FolderMovieInfo?.Languages ?? new List<Language>();
            }

            if (!languages.Any(l => l != Language.English))
            {
                languages = localMovie.FileMovieInfo?.Languages ?? new List<Language>();
            }

            if (!languages.Any())
            {
                languages.Add(Language.English);
            }

            _logger.Debug("Using languages: {0}", languages.Select(l => l.Name).ToList().Join(","));

            localMovie.Languages = languages.Distinct().ToList();

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
