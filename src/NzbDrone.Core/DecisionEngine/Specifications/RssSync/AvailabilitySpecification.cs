using System;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class AvailabilitySpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public AvailabilitySpecification(Logger logger)
        {
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

            MovieStatusType minAvail;
            int x = 0;
            bool result = Int32.TryParse(subject.Movie.Minimumavailability, out x);
            //should match cases from: src\NzbDrone.Core\Tv\MovieStatusType.cs
            switch (x)
            {
                case 1:
                    minAvail = MovieStatusType.Announced;
                    break;
                case 2:
                    minAvail = MovieStatusType.InCinemas;
                    break;
                case 3:
                    minAvail = MovieStatusType.Released;
                    break;
                default:
                    minAvail = MovieStatusType.Released;
                    break;
            }
            if (subject.Movie.Status < minAvail)
            {
                return Decision.Reject("Movie is not available");
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
