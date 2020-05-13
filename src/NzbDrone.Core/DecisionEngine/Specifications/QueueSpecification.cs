using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Queue;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class QueueSpecification : IDecisionEngineSpecification
    {
        private readonly IQueueService _queueService;
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly IPreferredWordService _preferredWordServiceCalculator;
        private readonly Logger _logger;

        public QueueSpecification(IQueueService queueService,
                                       UpgradableSpecification upgradableSpecification,
                                       IPreferredWordService preferredWordServiceCalculator,
                                       Logger logger)
        {
            _queueService = queueService;
            _upgradableSpecification = upgradableSpecification;
            _preferredWordServiceCalculator = preferredWordServiceCalculator;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteBook subject, SearchCriteriaBase searchCriteria)
        {
            var queue = _queueService.GetQueue();
            var matchingAlbum = queue.Where(q => q.RemoteBook?.Author != null &&
                                                 q.RemoteBook.Author.Id == subject.Author.Id &&
                                                 q.RemoteBook.Books.Select(e => e.Id).Intersect(subject.Books.Select(e => e.Id)).Any())
                           .ToList();

            foreach (var queueItem in matchingAlbum)
            {
                var remoteAlbum = queueItem.RemoteBook;
                var qualityProfile = subject.Author.QualityProfile.Value;

                // To avoid a race make sure it's not FailedPending (failed awaiting removal/search).
                // Failed items (already searching for a replacement) won't be part of the queue since
                // it's a copy, of the tracked download, not a reference.
                if (queueItem.TrackedDownloadState == TrackedDownloadState.DownloadFailedPending)
                {
                    continue;
                }

                _logger.Debug("Checking if existing release in queue meets cutoff. Queued quality is: {0}", remoteAlbum.ParsedBookInfo.Quality);

                var queuedItemPreferredWordScore = _preferredWordServiceCalculator.Calculate(subject.Author, queueItem.Title);

                if (!_upgradableSpecification.CutoffNotMet(qualityProfile,
                                                           new List<QualityModel> { remoteAlbum.ParsedBookInfo.Quality },
                                                           queuedItemPreferredWordScore,
                                                           subject.ParsedBookInfo.Quality,
                                                           subject.PreferredWordScore))
                {
                    return Decision.Reject("Release in queue already meets cutoff: {0}", remoteAlbum.ParsedBookInfo.Quality);
                }

                _logger.Debug("Checking if release is higher quality than queued release. Queued: {0}", remoteAlbum.ParsedBookInfo.Quality);

                if (!_upgradableSpecification.IsUpgradable(qualityProfile,
                                                           new List<QualityModel> { remoteAlbum.ParsedBookInfo.Quality },
                                                           queuedItemPreferredWordScore,
                                                           subject.ParsedBookInfo.Quality,
                                                           subject.PreferredWordScore))
                {
                    return Decision.Reject("Release in queue is of equal or higher preference: {0}", remoteAlbum.ParsedBookInfo.Quality);
                }

                _logger.Debug("Checking if profiles allow upgrading. Queued: {0}", remoteAlbum.ParsedBookInfo.Quality);

                if (!_upgradableSpecification.IsUpgradeAllowed(qualityProfile,
                                                               new List<QualityModel> { remoteAlbum.ParsedBookInfo.Quality },
                                                               subject.ParsedBookInfo.Quality))
                {
                    return Decision.Reject("Another release is queued and the Quality profile does not allow upgrades");
                }
            }

            return Decision.Accept();
        }
    }
}
