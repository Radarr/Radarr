using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class AuthorSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public AuthorSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteBook remoteAlbum, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria == null)
            {
                return Decision.Accept();
            }

            _logger.Debug("Checking if author matches searched author");

            if (remoteAlbum.Author.Id != searchCriteria.Author.Id)
            {
                _logger.Debug("Author {0} does not match {1}", remoteAlbum.Author, searchCriteria.Author);
                return Decision.Reject("Wrong author");
            }

            return Decision.Accept();
        }
    }
}
