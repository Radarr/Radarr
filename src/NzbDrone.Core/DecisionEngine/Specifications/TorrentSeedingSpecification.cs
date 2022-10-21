using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class TorrentSeedingSpecification : IDecisionEngineSpecification
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly Logger _logger;

        public TorrentSeedingSpecification(IIndexerFactory indexerFactory, Logger logger)
        {
            _indexerFactory = indexerFactory;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public IEnumerable<Decision> IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            return new List<Decision> { Calculate(subject, searchCriteria) };
        }

        private Decision Calculate(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var torrentInfo = subject.Release as TorrentInfo;

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
                var minimumSeeders = torrentIndexerSettings.MinimumSeeders;

                if (torrentInfo.Seeders.HasValue && torrentInfo.Seeders.Value < minimumSeeders)
                {
                    _logger.Debug("Not enough seeders: {0}. Minimum seeders: {1}", torrentInfo.Seeders, minimumSeeders);
                    return Decision.Reject(string.Format("Not enough seeders: {0}. Minimum seeders: {1}", torrentInfo.Seeders, minimumSeeders));
                }
            }

            return Decision.Accept();
        }
    }
}
