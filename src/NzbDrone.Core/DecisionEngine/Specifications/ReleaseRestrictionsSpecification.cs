using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Restrictions;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class ReleaseRestrictionsSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IRestrictionService _restrictionService;
        private readonly ITermMatcher _termMatcher;

        public ReleaseRestrictionsSpecification(ITermMatcher termMatcher, IRestrictionService restrictionService, Logger logger)
        {
            _logger = logger;
            _restrictionService = restrictionService;
            _termMatcher = termMatcher;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual IEnumerable<Decision> IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            return new List<Decision> { Calculate(subject, searchCriteria) };
        }

        private Decision Calculate(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            _logger.Debug("Checking if release meets restrictions: {0}", subject);

            var title = subject.Release.Title;
            var restrictions = _restrictionService.AllForTags(subject.Movie.Tags);

            var required = restrictions.Where(r => r.Required.IsNotNullOrWhiteSpace());
            var ignored = restrictions.Where(r => r.Ignored.IsNotNullOrWhiteSpace());

            foreach (var r in required)
            {
                var requiredTerms = r.Required.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                var foundTerms = ContainsAny(requiredTerms, title);
                if (foundTerms.Empty())
                {
                    var terms = string.Join(", ", requiredTerms);
                    _logger.Debug("[{0}] does not contain one of the required terms: {1}", title, terms);
                    return Decision.Reject(string.Format("Does not contain one of the required terms: {0}", terms));
                }
            }

            foreach (var r in ignored)
            {
                var ignoredTerms = r.Ignored.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                var foundTerms = ContainsAny(ignoredTerms, title);
                if (foundTerms.Any())
                {
                    var terms = string.Join(", ", foundTerms);
                    _logger.Debug("[{0}] contains these ignored terms: {1}", title, terms);
                    return Decision.Reject(string.Format("Contains these ignored terms: {0}", terms));
                }
            }

            _logger.Debug("[{0}] No restrictions apply, allowing", subject);
            return Decision.Accept();
        }

        private List<string> ContainsAny(List<string> terms, string title)
        {
            return terms.Where(t => _termMatcher.IsMatch(t, title)).ToList();
        }
    }
}
