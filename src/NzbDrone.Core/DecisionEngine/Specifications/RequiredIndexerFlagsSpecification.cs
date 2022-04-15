using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
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

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public IEnumerable<Decision> IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            return new List<Decision> { Calculate(subject, searchCriteria) };
        }

        private Decision Calculate(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var torrentInfo = subject.Release;

            IIndexerSettings indexerSettings = null;
            try
            {
                indexerSettings = _indexerFactory.Get(subject.Release.IndexerId)?.Settings as IIndexerSettings;
            }
            catch (Exception)
            {
                _logger.Debug("Indexer with id {0} does not exist, skipping required indexer flags specs.", subject.Release.IndexerId);
            }

            if (torrentInfo == null || indexerSettings == null)
            {
                return Decision.Accept();
            }

            if (indexerSettings is ITorrentIndexerSettings torrentIndexerSettings)
            {
                var requiredFlags = torrentIndexerSettings.RequiredFlags;
                var requiredFlag = (IndexerFlags)0;

                if (requiredFlags == null || !requiredFlags.Any())
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
                return Decision.Reject(string.Format("None of the required indexer flags {0} where found. Found flags: {1}", requiredFlag, torrentInfo.IndexerFlags));
            }

            return Decision.Accept();
        }
    }
}
