using System;
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
        private readonly QualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly Logger _logger;

        public QueueSpecification(IQueueService queueService,
                                       QualityUpgradableSpecification qualityUpgradableSpecification,
                                       Logger logger)
        {
            _queueService = queueService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            var queue = _queueService.GetQueue()
                            .Select(q => q.RemoteAlbum).ToList();

            var matchingArtist = queue.Where(q => q.Artist.Id == subject.Artist.Id);
            var matchingAlbum = matchingArtist.Where(q => q.Albums.Select(e => e.Id).Intersect(subject.Albums.Select(e => e.Id)).Any());

            foreach (var remoteAlbum in matchingAlbum)
            {
                _logger.Debug("Checking if existing release in queue meets cutoff. Queued quality is: {0}", remoteAlbum.ParsedAlbumInfo.Quality);

                if (!_qualityUpgradableSpecification.CutoffNotMet(subject.Artist.Profile, remoteAlbum.ParsedAlbumInfo.Quality, subject.ParsedAlbumInfo.Quality))
                {
                    return Decision.Reject("Quality for release in queue already meets cutoff: {0}", remoteAlbum.ParsedAlbumInfo.Quality);
                }

                _logger.Debug("Checking if release is higher quality than queued release. Queued quality is: {0}", remoteAlbum.ParsedAlbumInfo.Quality);

                if (!_qualityUpgradableSpecification.IsUpgradable(subject.Artist.Profile, remoteAlbum.ParsedAlbumInfo.Quality, subject.ParsedAlbumInfo.Quality))
                {
                    return Decision.Reject("Quality for release in queue is of equal or higher preference: {0}", remoteAlbum.ParsedAlbumInfo.Quality);
                }
            }

            return Decision.Accept();

        }
    }
}
