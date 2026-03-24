using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class MovieSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MovieSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria == null)
            {
                return DownloadSpecDecision.Accept();
            }

            _logger.Debug("Checking if movie matches searched movie");

            if (subject.Movie.Id != searchCriteria.Movie.Id)
            {
                _logger.Debug("Movie {0} does not match {1}", subject.Movie, searchCriteria.Movie);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongMovie, "Wrong movie");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
