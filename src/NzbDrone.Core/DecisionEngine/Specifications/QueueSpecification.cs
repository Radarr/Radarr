using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Queue;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class QueueSpecification : IDecisionEngineSpecification
    {
        private readonly IQueueService _queueService;
        private readonly UpgradableSpecification _qualityUpgradableSpecification;
        private readonly Logger _logger;

        public QueueSpecification(IQueueService queueService,
                                       UpgradableSpecification qualityUpgradableSpecification,
                                       Logger logger)
        {
            _queueService = queueService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
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

                _logger.Debug("Checking if existing release in queue meets cutoff. Queued quality is: {0}", remoteMovie.ParsedMovieInfo.Quality);

                if (!_qualityUpgradableSpecification.CutoffNotMet(qualityProfile, remoteMovie.ParsedMovieInfo.Quality, subject.ParsedMovieInfo.Quality))
                {
                    return Decision.Reject("Quality for release in queue already meets cutoff: {0}", remoteMovie.ParsedMovieInfo.Quality);
                }

                _logger.Debug("Checking if release is higher quality than queued release. Queued quality is: {0}", remoteMovie.ParsedMovieInfo.Quality);

                if (!_qualityUpgradableSpecification.IsUpgradable(qualityProfile, remoteMovie.ParsedMovieInfo.Quality, subject.ParsedMovieInfo.Quality))
                {
                    return Decision.Reject("Quality for release in queue is of equal or higher preference: {0}", remoteMovie.ParsedMovieInfo.Quality);
                }

                _logger.Debug("Checking if profiles allow upgrading. Queued: {0}", remoteMovie.ParsedMovieInfo.Quality);

                if (!_qualityUpgradableSpecification.IsUpgradeAllowed(subject.Movie.Profile,
                                                               remoteMovie.ParsedMovieInfo.Quality,
                                                               subject.ParsedMovieInfo.Quality))
                {
                    return Decision.Reject("Another release is queued and the Quality profile does not allow upgrades");
                }
            }

            return Decision.Accept();
        }
    }
}
