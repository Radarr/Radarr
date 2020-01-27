using NLog;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class BlacklistSpecification : IDecisionEngineSpecification
    {
        private readonly IBlacklistService _blacklistService;
        private readonly Logger _logger;

        public BlacklistSpecification(IBlacklistService blacklistService, Logger logger)
        {
            _blacklistService = blacklistService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            if (_blacklistService.Blacklisted(subject.Movie.Id, subject.Release))
            {
                _logger.Debug("{0} is blacklisted, rejecting.", subject.Release.Title);
                return Decision.Reject("Release is blacklisted");
            }

            return Decision.Accept();
        }
    }
}
