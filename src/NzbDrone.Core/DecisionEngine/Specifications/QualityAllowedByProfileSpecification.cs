using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class QualityAllowedByProfileSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public QualityAllowedByProfileSpecification(Logger logger)
        {
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        //TODO: Update tests!
        public virtual Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            _logger.Debug("Checking if report meets quality requirements. {0}", subject.ParsedMovieInfo.Quality);
            if (!subject.Movie.Profile.Value.Items.Exists(v => v.Allowed && v.QualityDefinition == subject.ParsedMovieInfo.Quality.QualityDefinition))
            {
                _logger.Debug("Quality {0} rejected by Movies' quality profile", subject.ParsedMovieInfo.Quality);
                return Decision.Reject("{0} is not wanted in profile", subject.ParsedMovieInfo.Quality.QualityDefinition);
            }

            return Decision.Accept();
        }
    }
}
