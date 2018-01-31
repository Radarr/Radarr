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
            
            _logger.Debug("Checking if report meets language requirements. {0}", subject.ParsedEpisodeInfo.Language);

            if (subject.Series.Profile.Value.AllowMulti && subject.ParsedEpisodeInfo.IsMulti)
                return Decision.Accept();

            if (subject.ParsedEpisodeInfo.Language != wantedLanguage)
            {
                _logger.Debug("Report Language: {0} rejected because it is not wanted, wanted {1}", subject.ParsedEpisodeInfo.Language, wantedLanguage);
                return Decision.Reject("{0} is wanted, but found {1}", wantedLanguage, subject.ParsedEpisodeInfo.Language);
            }

            return Decision.Accept();
        }

        public virtual Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var wantedLanguage = subject.Movie.Profile.Value.Language;

            _logger.Debug("Checking if report meets language requirements. {0}", subject.ParsedMovieInfo.Language);

            if (subject.Movie.Profile.Value.AllowMulti && subject.ParsedMovieInfo.IsMulti)
                return Decision.Accept();

            if (subject.ParsedMovieInfo.Language != wantedLanguage)
            {
                _logger.Debug("Report Language: {0} rejected because it is not wanted, wanted {1}", subject.ParsedMovieInfo.Language, wantedLanguage);
                return Decision.Reject("{0} is wanted, but found {1}", wantedLanguage, subject.ParsedMovieInfo.Language);
            }

            return Decision.Accept();
        }
    }
}
