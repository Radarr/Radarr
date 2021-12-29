using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Queue;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class QueueSpecification : IDecisionEngineSpecification
    {
        private readonly IQueueService _queueService;
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly Logger _logger;

        public QueueSpecification(IQueueService queueService,
                                  UpgradableSpecification upgradableSpecification,
                                  ICustomFormatCalculationService formatService,
                                  Logger logger)
        {
            _queueService = queueService;
            _upgradableSpecification = upgradableSpecification;
            _formatService = formatService;
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
                var qualityProfile = subject.Movie.Profile;

                // To avoid a race make sure it's not FailedPending (failed awaiting removal/search).
                // Failed items (already searching for a replacement) won't be part of the queue since
                // it's a copy, of the tracked download, not a reference.
                if (queueItem.TrackedDownloadState == TrackedDownloadState.FailedPending)
                {
                    continue;
                }

                var customFormats = _formatService.ParseCustomFormat(remoteMovie.ParsedMovieInfo, subject.Movie);

                _logger.Debug("Checking if existing release in queue meets cutoff. Queued quality is: {0} - {1}",
                              remoteMovie.ParsedMovieInfo.Quality,
                              customFormats.ConcatToString());

                if (!_upgradableSpecification.CutoffNotMet(qualityProfile,
                                                           remoteMovie.ParsedMovieInfo.Quality,
                                                           customFormats,
                                                           subject.ParsedMovieInfo.Quality))
                {
                    return Decision.Reject("Quality for release in queue already meets cutoff: {0}", remoteMovie.ParsedMovieInfo.Quality);
                }

                _logger.Debug("Checking if release is higher quality than queued release. Queued quality is: {0}", remoteMovie.ParsedMovieInfo.Quality);

                if (!_upgradableSpecification.IsUpgradable(qualityProfile,
                                                           remoteMovie.ParsedMovieInfo.Quality,
                                                           remoteMovie.CustomFormats,
                                                           subject.ParsedMovieInfo.Quality,
                                                           subject.CustomFormats))
                {
                    return Decision.Reject("Quality for release in queue is of equal or higher preference: {0}", remoteMovie.ParsedMovieInfo.Quality);
                }

                _logger.Debug("Checking if profiles allow upgrading. Queued: {0}", remoteMovie.ParsedMovieInfo.Quality);

                if (!_upgradableSpecification.IsUpgradeAllowed(subject.Movie.Profile,
                                                               remoteMovie.ParsedMovieInfo.Quality,
                                                               remoteMovie.CustomFormats,
                                                               subject.ParsedMovieInfo.Quality,
                                                               subject.CustomFormats))
                {
                    return Decision.Reject("Another release is queued and the Quality profile does not allow upgrades");
                }
            }

            return Decision.Accept();
        }
    }
}
