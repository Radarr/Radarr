using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class MovieSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MovieSpecification(Logger logger)
        {
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria == null)
            {
                return Decision.Accept();
            }

            _logger.Debug("Checking if movie matches searched movie");

            if (subject.Movie.Id != searchCriteria.Movie.Id)
            {
                _logger.Debug("Movie {0} does not match {1}", subject.Movie, searchCriteria.Movie);
                return Decision.Reject("Wrong movie");
            }

            return Decision.Accept();
        }
    }
}
