using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Language;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators
{
    public class AggregateLanguage : IAggregateLocalMovie
    {
        private readonly List<IAugmentLanguage> _augmentLanguages;
        private readonly Logger _logger;

        public AggregateLanguage(IEnumerable<IAugmentLanguage> augmentLanguages,
                                Logger logger)
        {
            _augmentLanguages = augmentLanguages.OrderBy(a => a.Order).ToList();
            _logger = logger;
        }

        public LocalMovie Aggregate(LocalMovie localMovie, DownloadClientItem downloadClientItem, bool otherFiles)
        {
            var languages = new List<Language> { localMovie.Movie?.OriginalLanguage ?? Language.Unknown };
            var languagesConfidence = Confidence.Default;

            foreach (var augmentLanguage in _augmentLanguages)
            {
                var augmentedLanguage = augmentLanguage.AugmentLanguage(localMovie, downloadClientItem);
                if (augmentedLanguage == null)
                {
                    continue;
                }

                _logger.Trace("Considering Languages {0} ({1}) from {2}", string.Join(", ", augmentedLanguage.Languages ?? new List<Language>()), augmentedLanguage.Confidence, augmentLanguage.Name);

                if (augmentedLanguage?.Languages != null && augmentedLanguage.Languages.Count > 0 && !(augmentedLanguage.Languages.Count == 1 && augmentedLanguage.Languages.Contains(Language.Unknown)))
                {
                    languages = augmentedLanguage.Languages;
                    languagesConfidence = augmentedLanguage.Confidence;
                }
            }

            _logger.Debug("Selected languages: {0}", string.Join(", ", languages.ToList()));

            localMovie.Languages = languages;

            return localMovie;
        }
    }
}
