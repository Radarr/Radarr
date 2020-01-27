using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.History;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class AlreadyImportedSpecification : IDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public AlreadyImportedSpecification(IHistoryService historyService,
                                            IConfigService configService,
                                            Logger logger)
        {
            _historyService = historyService;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteMovie subject, SearchCriteriaBase searchCriteria)
        {
            var cdhEnabled = _configService.EnableCompletedDownloadHandling;

            if (!cdhEnabled)
            {
                _logger.Debug("Skipping already imported check because CDH is disabled");
                return Decision.Accept();
            }

            var movie = subject.Movie;

            _logger.Debug("Performing already imported check on report");
            if (movie != null)
            {
                if (!movie.HasFile)
                {
                    _logger.Debug("Skipping already imported check for episode without file");
                    return Decision.Accept();
                }

                var historyForEpisode = _historyService.GetByMovieId(movie.Id, null);
                var lastGrabbed = historyForEpisode.FirstOrDefault(h => h.EventType == HistoryEventType.Grabbed);

                if (lastGrabbed == null)
                {
                    return Decision.Accept();
                }

                var imported = historyForEpisode.FirstOrDefault(h =>
                    h.EventType == HistoryEventType.DownloadFolderImported &&
                    h.DownloadId == lastGrabbed.DownloadId);

                if (imported == null)
                {
                    return Decision.Accept();
                }

                // This is really only a guard against redownloading the same release over
                // and over when the grabbed and imported qualities do not match, if they do
                // match skip this check.
                if (lastGrabbed.Quality.Equals(imported.Quality))
                {
                    return Decision.Accept();
                }

                var release = subject.Release;

                if (release.DownloadProtocol == DownloadProtocol.Torrent)
                {
                    var torrentInfo = release as TorrentInfo;

                    if (torrentInfo?.InfoHash != null && torrentInfo.InfoHash.ToUpper() == lastGrabbed.DownloadId)
                    {
                        _logger.Debug("Has same torrent hash as a grabbed and imported release");
                        return Decision.Reject("Has same torrent hash as a grabbed and imported release");
                    }
                }

                // Only based on title because a release with the same title on another indexer/released at
                // a different time very likely has the exact same content and we don't need to also try it.
                if (release.Title.Equals(lastGrabbed.SourceTitle, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.Debug("Has same release name as a grabbed and imported release");
                    return Decision.Reject("Has same release name as a grabbed and imported release");
                }
            }

            return Decision.Accept();
        }
    }
}
