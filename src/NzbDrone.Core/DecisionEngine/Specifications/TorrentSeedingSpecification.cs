using System;
using NLog;
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

        //public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var torrentInfo = subject.Release as TorrentInfo;

            IIndexerSettings indexerSettings = null;
            try {
                indexerSettings = _indexerFactory.Get(subject.Release.IndexerId)?.Settings as IIndexerSettings;
            }
            catch (Exception e)
            {
                _logger.Debug("Indexer with id {0} does not exist, skipping minimum seeder checks.", subject.Release.IndexerId);
            }


            if (torrentInfo == null || indexerSettings == null)
            {
                return Decision.Accept();
            }

            if (indexerSettings is ITorrentIndexerSettings torrentIndexerSettings)
            {
                var minimumSeeders = torrentIndexerSettings.MinimumSeeders;

                if (torrentInfo.Seeders.HasValue && torrentInfo.Seeders.Value < minimumSeeders)
                {
                    _logger.Debug("Not enough seeders: {0}. Minimum seeders: {1}", torrentInfo.Seeders, minimumSeeders);
                    return Decision.Reject("Not enough seeders: {0}. Minimum seeders: {1}", torrentInfo.Seeders, minimumSeeders);
                }
            }

            return Decision.Accept();
        }
    }
}
