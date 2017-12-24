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
        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly Logger _logger;

        public QueueSpecification(IQueueService queueService,
                                       UpgradableSpecification upgradableSpecification,
                                       Logger logger)
        {
            _queueService = queueService;
            _upgradableSpecification = upgradableSpecification;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            var queue = _queueService.GetQueue()
                            .Select(q => q.RemoteAlbum).ToList();

            var matchingArtist = queue.Where(q => q.Artist.Id == subject.Artist.Id);
            var matchingAlbum = matchingArtist.Where(q => q.Albums.Select(e => e.Id).Intersect(subject.Albums.Select(e => e.Id)).Any());

            foreach (var remoteAlbum in matchingAlbum)
            {
                _logger.Debug("Checking if existing release in queue meets cutoff. Queued quality is: {0} - {1}", remoteAlbum.ParsedAlbumInfo.Quality, remoteAlbum.ParsedAlbumInfo.Language);

                if (!_upgradableSpecification.CutoffNotMet(subject.Artist.Profile,
                                                           subject.Artist.LanguageProfile,
                                                           remoteAlbum.ParsedAlbumInfo.Quality,
                                                           remoteAlbum.ParsedAlbumInfo.Language,
                                                           subject.ParsedAlbumInfo.Quality))
                {
                    return Decision.Reject("Quality for release in queue already meets cutoff: {0}", remoteAlbum.ParsedAlbumInfo.Quality);
                }

                _logger.Debug("Checking if release is higher quality than queued release. Queued quality is: {0} - {1}", remoteAlbum.ParsedAlbumInfo.Quality, remoteAlbum.ParsedAlbumInfo.Language);

                if (!_upgradableSpecification.IsUpgradable(subject.Artist.Profile,
                                                           subject.Artist.LanguageProfile,
                                                           remoteAlbum.ParsedAlbumInfo.Quality,
                                                           remoteAlbum.ParsedAlbumInfo.Language,
                                                           subject.ParsedAlbumInfo.Quality,
                                                           subject.ParsedAlbumInfo.Language))
                {
                    return Decision.Reject("Quality for release in queue is of equal or higher preference: {0} - {1}", remoteAlbum.ParsedAlbumInfo.Quality, remoteAlbum.ParsedAlbumInfo.Language);
                }
            }

            return Decision.Accept();

        }
    }
}
