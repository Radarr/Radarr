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

        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            if (searchCriteria != null)
            {
                if (!searchCriteria.MonitoredEpisodesOnly)
                {
                    _logger.Debug("Skipping monitored check during search");
                    return Decision.Accept();
                }
            }

            if (!subject.Artist.Monitored)
            {
                _logger.Debug("{0} is present in the DB but not tracked. skipping.", subject.Artist);
                return Decision.Reject("Artist is not monitored");
            }

            var monitoredCount = subject.Albums.Count(album => album.Monitored);
            if (monitoredCount == subject.Albums.Count)
            {
                return Decision.Accept();
            }

            _logger.Debug("Only {0}/{1} albums are monitored. skipping.", monitoredCount, subject.Albums.Count);
            return Decision.Reject("Album is not monitored");
        }
    }
}
