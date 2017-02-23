using System;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Configuration;

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

        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
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
                return Decision.Reject("Movie {0} will only be considered available {1} days after {2}", subject.Movie, _settingsService.AvailabilityDelay, subject.Movie.MinimumAvailability.ToString());
            }

            return Decision.Accept();
        }

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
            {
                if (!searchCriteria.MonitoredEpisodesOnly)
                {
                    _logger.Debug("Skipping availability check during search");
                    return Decision.Accept();
                }
            }

            /*if (subject.Series.Status != MovieStatusType.Released)
            {
                _logger.Debug("{0} is present in the DB but not yet available. skipping.", subject.Series);
                return Decision.Reject("Series is not yet available");
            }

            /*var monitoredCount = subject.Episodes.Count(episode => episode.Monitored);
            if (monitoredCount == subject.Episodes.Count)
            {
                return Decision.Accept();
            }

            _logger.Debug("Only {0}/{1} episodes are monitored. skipping.", monitoredCount, subject.Episodes.Count);*/
            return Decision.Reject("Episode is not yet available");
        }
    }
}
