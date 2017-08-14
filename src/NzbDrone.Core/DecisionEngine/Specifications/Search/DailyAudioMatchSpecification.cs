using System;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class DailyAudioMatchSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public DailyAudioMatchSpecification(Logger logger)
        {
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            throw new NotImplementedException();

            // TODO Rework for Daily Audio/Podcasts
        }
    }
}