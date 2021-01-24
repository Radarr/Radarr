using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

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

        public virtual Decision IsSatisfiedBy(RemoteBook subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.ParsedBookInfo.Discography)
            {
                _logger.Debug("Checking if all books in discography release have released. {0}", subject.Release.Title);

                if (subject.Books.Any(e => !e.ReleaseDate.HasValue || e.ReleaseDate.Value.After(DateTime.UtcNow)))
                {
                    _logger.Debug("Discography release {0} rejected. All books haven't released yet.", subject.Release.Title);
                    return Decision.Reject("Discography release rejected. All books haven't released yet.");
                }
            }

            return Decision.Accept();
        }
    }
}
