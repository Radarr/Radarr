using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class EarlyReleaseSpecification : IDecisionEngineSpecification
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly Logger _logger;

        public EarlyReleaseSpecification(IIndexerFactory indexerFactory, Logger logger)
        {
            _indexerFactory = indexerFactory;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteBook subject, SearchCriteriaBase searchCriteria)
        {
            var releaseInfo = subject.Release;

            if (releaseInfo == null || releaseInfo.IndexerId == 0)
            {
                return Decision.Accept();
            }

            IndexerDefinition indexer;
            try
            {
                indexer = _indexerFactory.Get(subject.Release.IndexerId);
            }
            catch (ModelNotFoundException)
            {
                _logger.Debug("Indexer with id {0} does not exist, skipping early release check", subject.Release.IndexerId);
                return Decision.Accept();
            }

            var indexerSettings = indexer.Settings as IIndexerSettings;

            if (subject.Books.Count != 1 || indexerSettings?.EarlyReleaseLimit == null)
            {
                return Decision.Accept();
            }

            var releaseDate = subject.Books.First().ReleaseDate;

            if (releaseDate == null)
            {
                return Decision.Accept();
            }

            var isEarly = releaseDate.Value > subject.Release.PublishDate.AddDays(indexerSettings.EarlyReleaseLimit.Value);

            if (isEarly)
            {
                var message = $"Release published date, {subject.Release.PublishDate.ToShortDateString()}, is outside of {indexerSettings.EarlyReleaseLimit.Value} day early grab limit allowed by user";

                _logger.Debug(message);
                return Decision.Reject(message);
            }

            return Decision.Accept();
        }
    }
}
