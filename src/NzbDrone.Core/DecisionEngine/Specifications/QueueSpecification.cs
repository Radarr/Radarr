using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
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

        public IEnumerable<Decision> IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var queue = _queueService.GetQueue();
            var matchingMovies = queue.Where(q => q.RemoteMovie?.Movie != null &&
                                                   q.RemoteMovie.Movie.Id == subject.Movie.Id)
                                       .ToList();

            foreach (var queueItem in matchingMovies)
            {
                var remoteMovie = queueItem.RemoteMovie;

                // To avoid a race make sure it's not FailedPending (failed awaiting removal/search).
                // Failed items (already searching for a replacement) won't be part of the queue since
                // it's a copy, of the tracked download, not a reference.
                if (queueItem.TrackedDownloadState == TrackedDownloadState.FailedPending)
                {
                    continue;
                }

                var customFormats = _formatService.ParseCustomFormat(remoteMovie.ParsedMovieInfo, subject.Movie);

                foreach (var qualityProfile in subject.Movie.QualityProfiles.Value)
                {
                    yield return Calculate(qualityProfile, subject, remoteMovie, customFormats);
                }
            }
        }

        private Decision Calculate(Profile profile, RemoteMovie subject, RemoteMovie remoteMovie, List<CustomFormat> customFormats)
        {
            _logger.Debug("Checking if existing release in queue meets cutoff. Queued quality is: {0} - {1}",
              remoteMovie.ParsedMovieInfo.Quality,
              customFormats.ConcatToString());

            if (!_upgradableSpecification.CutoffNotMet(profile,
                                                       remoteMovie.ParsedMovieInfo.Quality,
                                                       customFormats,
                                                       subject.ParsedMovieInfo.Quality))
            {
                return Decision.Reject(string.Format("Quality for release in queue already meets cutoff: {0}", remoteMovie.ParsedMovieInfo.Quality), profile.Id);
            }

            _logger.Debug("Checking if release is higher quality than queued release. Queued quality is: {0}", remoteMovie.ParsedMovieInfo.Quality);

            if (!_upgradableSpecification.IsUpgradable(profile,
                                                       remoteMovie.ParsedMovieInfo.Quality,
                                                       remoteMovie.CustomFormats,
                                                       subject.ParsedMovieInfo.Quality,
                                                       subject.CustomFormats))
            {
                return Decision.Reject(string.Format("Quality for release in queue is of equal or higher preference: {0}", remoteMovie.ParsedMovieInfo.Quality), profile.Id);
            }

            _logger.Debug("Checking if profiles allow upgrading. Queued: {0}", remoteMovie.ParsedMovieInfo.Quality);

            if (!_upgradableSpecification.IsUpgradeAllowed(profile,
                                                           remoteMovie.ParsedMovieInfo.Quality,
                                                           remoteMovie.CustomFormats,
                                                           subject.ParsedMovieInfo.Quality,
                                                           subject.CustomFormats))
            {
                return Decision.Reject("Another release is queued and the Quality profile does not allow upgrades", profile.Id);
            }

            return Decision.Accept();
        }
    }
}
