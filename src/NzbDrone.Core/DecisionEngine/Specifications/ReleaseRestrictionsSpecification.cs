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

        public virtual Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            _logger.Debug("Checking if release meets restrictions: {0}", subject);

            var title = subject.Release.Title;
            var restrictions = _restrictionService.AllForTags(subject.Movie.Tags);
            var flags = subject.Release.IndexerFlags;

            var required = restrictions.Where(r => r.Required.IsNotNullOrWhiteSpace());
            var ignored = restrictions.Where(r => r.Ignored.IsNotNullOrWhiteSpace());

            foreach (var r in required)
            {
                var requiredTerms = r.Required.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                var foundTerms = ContainsAny(requiredTerms, title, flags);
                if (foundTerms.Empty())
                {
                    var terms = string.Join(", ", requiredTerms);
                    _logger.Debug("Title [{0}] and release flags [{1}] do not contain one of the required terms: {2}", title, flags, terms);
                    return Decision.Reject("Does not contain one of the required terms: {0}", terms);
                }
            }

            foreach (var r in ignored)
            {
                var ignoredTerms = r.Ignored.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                var foundTerms = ContainsAny(ignoredTerms, title, flags);
                if (foundTerms.Any())
                {
                    var terms = string.Join(", ", foundTerms);
                    _logger.Debug("Title [{0}] or release flags [{1}] contain these ignored terms: {2}", title, flags, terms);
                    return Decision.Reject("Contains these ignored terms: {0}", terms); //TODO: Add matched flags
                }
            }

            _logger.Debug("[{0}] No restrictions apply, allowing", subject);
            return Decision.Accept();
        }

        private IEnumerable<string> ContainsAny(List<string> terms, string title, IndexerFlags flags)
        {
            return MatchTitle(terms, title).Concat(MatchFlags(terms, flags)).Distinct();
        }

        private IEnumerable<string> MatchTitle(List<string> terms, string title)
        {
            return terms.Where(t => _termMatcher.IsMatch(t, title)).ToList();
        }

        private IEnumerable<string> MatchFlags(List<string> terms, IndexerFlags flags)
        {
            foreach (IndexerFlags f in Enum.GetValues(typeof(IndexerFlags)))
            {
                if (flags.HasFlag(f))
                {
                    foreach (var matchedTerm in terms.Where(t => _termMatcher.IsMatch(t, f.ToString())))
                    {
                        yield return matchedTerm;
                    }
                }
            }
        }
    }
}
