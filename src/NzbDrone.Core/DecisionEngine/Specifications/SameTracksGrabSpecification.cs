using System;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class SameTracksGrabSpecification : IDecisionEngineSpecification
    {
        private readonly SameTracksSpecification _sameTracksSpecification;
        private readonly Logger _logger;

        public SameTracksGrabSpecification(SameTracksSpecification sameTracksSpecification, Logger logger)
        {
            _sameTracksSpecification = sameTracksSpecification;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            throw new NotImplementedException();

            // TODO: Rework for Tracks if we can parse from release details.
        }
    }
}
