using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class LanguageSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public LanguageSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var wantedLanguage = subject.Movie.QualityProfile.Language;

            if (wantedLanguage == Language.Any)
            {
                _logger.Debug("Profile allows any language, accepting release.");
                return Decision.Accept();
            }

            var originalLanguage = subject.Movie.MovieMetadata.Value.OriginalLanguage;

            if (wantedLanguage == Language.Original)
            {
                if (!subject.Languages.Contains(originalLanguage))
                {
                    _logger.Debug("Original Language({0}) is wanted, but found {1}", originalLanguage, subject.Languages.ToExtendedString());
                    return Decision.Reject("Original Language ({0}) is wanted, but found {1}", originalLanguage, subject.Languages.ToExtendedString());
                }

                return Decision.Accept();
            }

            _logger.Debug("Checking if report meets language requirements. {0}", subject.ParsedMovieInfo.Languages.ToExtendedString());

            if (!subject.Languages.Contains(wantedLanguage))
            {
                _logger.Debug("Report Language: {0} rejected because it is not wanted, wanted {1}", subject.Languages.ToExtendedString(), wantedLanguage);
                return Decision.Reject("{0} is wanted, but found {1}", wantedLanguage, subject.Languages.ToExtendedString());
            }

            return Decision.Accept();
        }
    }
}
