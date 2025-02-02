using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class LanguageSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public LanguageSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var wantedLanguage = subject.Movie.QualityProfile.Language;

            if (wantedLanguage == Language.Any)
            {
                _logger.Debug("Profile allows any language, accepting release.");
                return DownloadSpecDecision.Accept();
            }

            var originalLanguage = subject.Movie.MovieMetadata.Value.OriginalLanguage;

            if (wantedLanguage == Language.Original)
            {
                if (!subject.Languages.Contains(originalLanguage))
                {
                    _logger.Debug("Original Language({0}) is wanted, but found {1}", originalLanguage, subject.Languages.ToExtendedString());
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.WantedLanguage, "Original Language ({0}) is wanted, but found {1}", originalLanguage, subject.Languages.ToExtendedString());
                }

                return DownloadSpecDecision.Accept();
            }

            _logger.Debug("Checking if report meets language requirements. {0}", subject.ParsedMovieInfo.Languages.ToExtendedString());

            if (!subject.Languages.Contains(wantedLanguage))
            {
                _logger.Debug("Report Language: {0} rejected because it is not wanted, wanted {1}", subject.Languages.ToExtendedString(), wantedLanguage);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.WantedLanguage, "{0} is wanted, but found {1}", wantedLanguage, subject.Languages.ToExtendedString());
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
