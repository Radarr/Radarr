using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class AvailabilitySpecification : IDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public AvailabilitySpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria is { UserInvokedSearch: true })
            {
                _logger.Debug("Skipping availability check during search");
                return Decision.Accept();
            }

            var availabilityDelay = _configService.AvailabilityDelay;

            if (!subject.Movie.IsAvailable(availabilityDelay))
            {
                return Decision.Reject("Movie {0} will only be considered available {1} days after {2}", subject.Movie, availabilityDelay, subject.Movie.MinimumAvailability.ToString());
            }

            return Decision.Accept();
        }
    }
}
