using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class RequiredIndexerFlagsSpecification : IDecisionEngineSpecification
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly Logger _logger;

        public RequiredIndexerFlagsSpecification(IIndexerFactory indexerFactory, Logger logger)
        {
            _indexerFactory = indexerFactory;
            _logger = logger;
        }

        //public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var torrentInfo = subject.Release;

            if (torrentInfo == null || torrentInfo.IndexerId == 0)
            {
                return Decision.Accept();
            }

            IndexerDefinition indexer;
            try
            {
                indexer = _indexerFactory.Get(torrentInfo.IndexerId);
            }
            catch (ModelNotFoundException)
            {
                _logger.Debug("Indexer with id {0} does not exist, skipping seeders check", torrentInfo.IndexerId);
                return Decision.Accept();
            }

            var torrentIndexerSettings = indexer.Settings as ITorrentIndexerSettings;

            if (torrentIndexerSettings != null)
            {
                var requiredFlags = torrentIndexerSettings.RequiredFlags;
                var requiredFlag = (IndexerFlags) 0;

                if (requiredFlags == null || requiredFlags.Count() == 0)
                {
                    return Decision.Accept();
                }
                
                foreach (var flag in requiredFlags)
                {
                    if (torrentInfo.IndexerFlags.HasFlag((IndexerFlags)flag))
                    {
                        return Decision.Accept();
                    }
                    requiredFlag |= (IndexerFlags)flag;
                }
                
                _logger.Debug("None of the required indexer flags {0} where found. Found flags: {1}", requiredFlag, torrentInfo.IndexerFlags);
                return Decision.Reject("None of the required indexer flags {0} where found. Found flags: {1}", requiredFlag, torrentInfo.IndexerFlags);
            }

            return Decision.Accept();
        }
    }
}
