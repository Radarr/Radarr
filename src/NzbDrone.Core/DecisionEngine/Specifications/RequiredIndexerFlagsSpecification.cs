using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class RequiredIndexerFlagsSpecification : IDownloadDecisionEngineSpecification
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

        public DownloadSpecDecision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
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
                return DownloadSpecDecision.Accept();
            }

            if (indexerSettings is ITorrentIndexerSettings torrentIndexerSettings)
            {
                var requiredFlags = torrentIndexerSettings.RequiredFlags;

                if (requiredFlags == null || !requiredFlags.Any())
                {
                    return DownloadSpecDecision.Accept();
                }

                var requiredFlag = (IndexerFlags)0;

                foreach (var flag in requiredFlags)
                {
                    if (torrentInfo.IndexerFlags.HasFlag((IndexerFlags)flag))
                    {
                        return DownloadSpecDecision.Accept();
                    }

                    requiredFlag |= (IndexerFlags)flag;
                }

                _logger.Debug("None of the required indexer flags {0} where found. Found flags: {1}", requiredFlag, torrentInfo.IndexerFlags);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.RequiredFlags, "None of the required indexer flags {0} where found. Found flags: {1}", requiredFlag, torrentInfo.IndexerFlags);
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
