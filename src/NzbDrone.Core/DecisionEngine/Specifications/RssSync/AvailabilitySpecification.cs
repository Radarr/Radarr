using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class AvailabilitySpecification : IDecisionEngineSpecification
    {
        private readonly IConfigService _settingsService;
        private readonly Logger _logger;

        public AvailabilitySpecification(IConfigService settingsService, Logger logger)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public IEnumerable<Decision> IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            return new List<Decision> { Calculate(subject, searchCriteria) };
        }

        private Decision Calculate(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
            {
                if (searchCriteria.UserInvokedSearch)
                {
                    _logger.Debug("Skipping availability check during search");
                    return Decision.Accept();
                }
            }

            if (!subject.Movie.IsAvailable(_settingsService.AvailabilityDelay))
            {
                return Decision.Reject(string.Format("Movie {0} will only be considered available {1} days after {2}", subject.Movie, _settingsService.AvailabilityDelay, subject.Movie.MinimumAvailability.ToString()));
            }

            return Decision.Accept();
        }
    }
}
