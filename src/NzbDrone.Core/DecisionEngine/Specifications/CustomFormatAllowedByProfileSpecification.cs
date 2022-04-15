using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class CustomFormatAllowedbyProfileSpecification : IDecisionEngineSpecification
    {
        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual IEnumerable<Decision> IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            foreach (var profile in subject.Movie.QualityProfiles.Value)
            {
                var minScore = profile.MinFormatScore;
                var score = subject.CustomFormatScore;

                if (score < minScore)
                {
                    yield return Decision.Reject(string.Format("Custom Formats {0} have score {1} below Movie's minimum {2}", subject.CustomFormats.ConcatToString(), score, minScore), profile.Id);
                }
                else
                {
                    yield return Decision.Accept();
                }
            }
        }
    }
}
