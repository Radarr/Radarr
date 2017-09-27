using System;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Common.Extensions;
using System.Linq;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class DiscographySpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public DiscographySpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            throw new NotImplementedException();
        }
    }
}
