using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
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

        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var wantedLanguage = subject.Series.Profile.Value.Language;

            if (subject.ParsedEpisodeInfo.Language == wantedLanguage || subject.ParsedEpisodeInfo.Language == Parser.Language.Hungarian)
            {

                return Decision.Accept();
            }
            else
            {
                _logger.Debug("Report Language: {0} rejected because it is not wanted", subject.ParsedEpisodeInfo.Language);
                return Decision.Reject("{0} isn't wanted!", subject.ParsedEpisodeInfo.Language);
            }
        }

        public virtual Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var wantedLanguage = Parser.Language.English;

            if (subject.ParsedMovieInfo.Language == wantedLanguage || subject.ParsedMovieInfo.Language == Parser.Language.Hungarian)
            {

                return Decision.Accept();
            }
            else
            {
                _logger.Debug("Report Language: {0} rejected because it is not wanted", subject.ParsedMovieInfo.Language);
                return Decision.Reject("{0} isn't wanted!", subject.ParsedMovieInfo.Language);
            }

        }
    }
}
