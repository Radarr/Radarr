using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;
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

        public Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            var queue = _queueService.GetQueue();
            var matchingAlbum = queue.Where(q => q.RemoteAlbum != null &&
                                       q.RemoteAlbum.Artist != null &&
                                       q.RemoteAlbum.Artist.Id == subject.Artist.Id &&
                                       q.RemoteAlbum.Albums.Select(e => e.Id).Intersect(subject.Albums.Select(e => e.Id)).Any())
                           .ToList();


            foreach (var queueItem in matchingAlbum)
            {
                var remoteAlbum = queueItem.RemoteAlbum;
                
                _logger.Debug("Checking if existing release in queue meets cutoff. Queued quality is: {0} - {1}", remoteAlbum.ParsedAlbumInfo.Quality, remoteAlbum.ParsedAlbumInfo.Language);
                var queuedItemPreferredWordScore = _preferredWordServiceCalculator.Calculate(subject.Artist, queueItem.Title);

                if (!_upgradableSpecification.CutoffNotMet(subject.Artist.QualityProfile,
                                                           subject.Artist.LanguageProfile,
                                                           remoteAlbum.ParsedAlbumInfo.Quality,
                                                           remoteAlbum.ParsedAlbumInfo.Language,
                                                           queuedItemPreferredWordScore,
                                                           subject.ParsedAlbumInfo.Quality,
                                                           subject.PreferredWordScore))

                {
                    return Decision.Reject("Quality for release in queue already meets cutoff: {0}", remoteAlbum.ParsedAlbumInfo.Quality);
                }

                _logger.Debug("Checking if release is higher quality than queued release. Queued quality is: {0} - {1}", remoteAlbum.ParsedAlbumInfo.Quality, remoteAlbum.ParsedAlbumInfo.Language);

                if (!_upgradableSpecification.IsUpgradable(subject.Artist.QualityProfile,
                                                           subject.Artist.LanguageProfile,
                                                           remoteAlbum.ParsedAlbumInfo.Quality,
                                                           remoteAlbum.ParsedAlbumInfo.Language,
                                                           queuedItemPreferredWordScore,
                                                           subject.ParsedAlbumInfo.Quality,
                                                           subject.ParsedAlbumInfo.Language,
                                                           subject.PreferredWordScore))
                {
                    return Decision.Reject("Quality for release in queue is of equal or higher preference: {0} - {1}", remoteAlbum.ParsedAlbumInfo.Quality, remoteAlbum.ParsedAlbumInfo.Language);
                }
            }

            return Decision.Accept();

        }
    }
}
