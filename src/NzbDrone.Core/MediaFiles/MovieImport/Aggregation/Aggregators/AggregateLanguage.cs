using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Language;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators
{
    public class AggregateLanguage : IAggregateLocalMovie
    {
        private readonly IEnumerable<IAugmentLanguage> _augmentQualities;
        private readonly Logger _logger;

        public AggregateLanguage(IEnumerable<IAugmentLanguage> augmentQualities,
                                Logger logger)
        {
            _augmentQualities = augmentQualities;
            _logger = logger;
        }

        public LocalMovie Aggregate(LocalMovie localMovie, bool otherFiles)
        {
            var augmentedLanguages = _augmentQualities.Select(a => a.AugmentLanguage(localMovie))
                                                      .Where(a => a != null)
                                                      .OrderBy(a => a.Confidence);

            var languages = new List<Language> { localMovie.Movie.OriginalLanguage ?? Language.Unknown };
            var languagesConfidence = Confidence.Default;

            foreach (var augmentedLanguage in augmentedLanguages)
            {
                if (augmentedLanguage?.Languages != null && augmentedLanguage.Languages.Count > 0 && !(augmentedLanguage.Languages.Count == 1 && augmentedLanguage.Languages.Contains(Language.Unknown)))
                {
                    languages = augmentedLanguage.Languages;
                    languagesConfidence = augmentedLanguage.Confidence;
                }
            }

            _logger.Debug("Using languages: {0}", string.Join(", ", languages.ToList()));

            localMovie.Languages = languages;

            return localMovie;
        }
    }
}
