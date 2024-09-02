using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Queue;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class QueueSpecification : IDecisionEngineSpecification
    {
        private readonly IQueueService _queueService;
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public QueueSpecification(IQueueService queueService,
                                  UpgradableSpecification upgradableSpecification,
                                  ICustomFormatCalculationService formatService,
                                  IConfigService configService,
                                  Logger logger)
        {
            _queueService = queueService;
            _upgradableSpecification = upgradableSpecification;
            _formatService = formatService;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var queue = _queueService.GetQueue();
            var matchingMovies = queue.Where(q => q.RemoteMovie?.Movie != null &&
                                                   q.RemoteMovie.Movie.Id == subject.Movie.Id)
                                       .ToList();

            foreach (var queueItem in matchingMovies)
            {
                var remoteMovie = queueItem.RemoteMovie;
                var qualityProfile = subject.Movie.QualityProfile;

                // To avoid a race make sure it's not FailedPending (failed awaiting removal/search).
                // Failed items (already searching for a replacement) won't be part of the queue since
                // it's a copy, of the tracked download, not a reference.
                if (queueItem.TrackedDownloadState == TrackedDownloadState.FailedPending)
                {
                    continue;
                }

                var queuedItemCustomFormats = _formatService.ParseCustomFormat(remoteMovie, (long)queueItem.Size);

                _logger.Debug("Checking if existing release in queue meets cutoff. Queued quality is: {0} - {1}",
                              remoteMovie.ParsedMovieInfo.Quality,
                              queuedItemCustomFormats.ConcatToString());

                if (!_upgradableSpecification.CutoffNotMet(qualityProfile,
                    remoteMovie.ParsedMovieInfo.Quality,
                    queuedItemCustomFormats,
                    subject.ParsedMovieInfo.Quality))
                {
                    return Decision.Reject("Quality for release in queue already meets cutoff: {0}", remoteMovie.ParsedMovieInfo.Quality);
                }

                _logger.Debug("Checking if release is higher quality than queued release. Queued quality is: {0}", remoteMovie.ParsedMovieInfo.Quality);

                var upgradeableRejectReason = _upgradableSpecification.IsUpgradable(qualityProfile,
                    remoteMovie.ParsedMovieInfo.Quality,
                    queuedItemCustomFormats,
                    subject.ParsedMovieInfo.Quality,
                    subject.CustomFormats);

                switch (upgradeableRejectReason)
                {
                    case UpgradeableRejectReason.BetterQuality:
                        return Decision.Reject("Release in queue on disk is of equal or higher preference: {0}", remoteMovie.ParsedMovieInfo.Quality);

                    case UpgradeableRejectReason.BetterRevision:
                        return Decision.Reject("Release in queue on disk is of equal or higher revision: {0}", remoteMovie.ParsedMovieInfo.Quality.Revision);

                    case UpgradeableRejectReason.QualityCutoff:
                        return Decision.Reject("Release in queue on disk meets quality cutoff: {0}", qualityProfile.Items[qualityProfile.GetIndex(qualityProfile.Cutoff).Index]);

                    case UpgradeableRejectReason.CustomFormatCutoff:
                        return Decision.Reject("Release in queue on disk meets Custom Format cutoff: {0}", qualityProfile.CutoffFormatScore);

                    case UpgradeableRejectReason.CustomFormatScore:
                        return Decision.Reject("Release in queue on disk has an equal or higher custom format score: {0}", qualityProfile.CalculateCustomFormatScore(queuedItemCustomFormats));
                }

                _logger.Debug("Checking if profiles allow upgrading. Queued: {0}", remoteMovie.ParsedMovieInfo.Quality);

                if (!_upgradableSpecification.IsUpgradeAllowed(subject.Movie.QualityProfile,
                                                               remoteMovie.ParsedMovieInfo.Quality,
                                                               remoteMovie.CustomFormats,
                                                               subject.ParsedMovieInfo.Quality,
                                                               subject.CustomFormats))
                {
                    return Decision.Reject("Another release is queued and the Quality profile does not allow upgrades");
                }

                if (_upgradableSpecification.IsRevisionUpgrade(remoteMovie.ParsedMovieInfo.Quality, subject.ParsedMovieInfo.Quality))
                {
                    if (_configService.DownloadPropersAndRepacks == ProperDownloadTypes.DoNotUpgrade)
                    {
                        _logger.Debug("Auto downloading of propers is disabled");
                        return Decision.Reject("Proper downloading is disabled");
                    }
                }
            }

            return Decision.Accept();
        }
    }
}
