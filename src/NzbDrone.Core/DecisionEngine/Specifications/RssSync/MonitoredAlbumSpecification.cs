using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class MonitoredAlbumSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MonitoredAlbumSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteBook subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
            {
                if (!searchCriteria.MonitoredBooksOnly)
                {
                    _logger.Debug("Skipping monitored check during search");
                    return Decision.Accept();
                }
            }

            if (!subject.Author.Monitored)
            {
                _logger.Debug("{0} is present in the DB but not tracked. Rejecting.", subject.Author);
                return Decision.Reject("Author is not monitored");
            }

            var monitoredCount = subject.Books.Count(book => book.Monitored);
            if (monitoredCount == subject.Books.Count)
            {
                return Decision.Accept();
            }

            if (subject.Books.Count == 1)
            {
                _logger.Debug("Album is not monitored. Rejecting", monitoredCount, subject.Books.Count);
                return Decision.Reject("Album is not monitored");
            }

            if (monitoredCount == 0)
            {
                _logger.Debug("No albums in the release are monitored. Rejecting", monitoredCount, subject.Books.Count);
            }
            else
            {
                _logger.Debug("Only {0}/{1} albums in the release are monitored. Rejecting", monitoredCount, subject.Books.Count);
            }

            return Decision.Reject("Album is not monitored");
        }
    }
}
