using System;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class DailyEpisodeMatchSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public DailyEpisodeMatchSpecification(Logger logger)
        {
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            throw new NotImplementedException();
        }
    }
}
