using System.Collections.Generic;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public interface IDecisionEngineSpecification
    {
        RejectionType Type { get; }

        SpecificationPriority Priority { get; }

        IEnumerable<Decision> IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria);
    }
}
