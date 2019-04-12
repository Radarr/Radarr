using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.History;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class AlreadyImportedSpecification : IDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly IConfigService _configService;
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;

        public AlreadyImportedSpecification(IHistoryService historyService,
                                            IConfigService configService,
                                            IMediaFileService mediaFileService,
                                            Logger logger)
        {
            _historyService = historyService;
            _mediaFileService = mediaFileService;
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Permanent;

        public Decision IsSatisfiedBy(RemoteAlbum subject, SearchCriteriaBase searchCriteria)
        {
            var cdhEnabled = _configService.EnableCompletedDownloadHandling;

            if (!cdhEnabled)
            {
                _logger.Debug("Skipping already imported check because CDH is disabled");
                return Decision.Accept();
            }

            _logger.Debug("Performing already imported check on report");
            foreach (var album in subject.Albums)
            {
                var trackFiles = _mediaFileService.GetFilesByAlbum(album.Id);

                if (trackFiles.Count() == 0)
                {
                    _logger.Debug("Skipping already imported check for album without files");
                    continue;
                }

                var historyForAlbum = _historyService.GetByAlbum(album.Id, null);
                var lastGrabbed = historyForAlbum.FirstOrDefault(h => h.EventType == HistoryEventType.Grabbed);

                if (lastGrabbed == null)
                {
                    continue;
                }

                var imported = historyForAlbum.FirstOrDefault(h =>
                    h.EventType == HistoryEventType.DownloadImported &&
                    h.DownloadId == lastGrabbed.DownloadId);

                if (imported == null)
                {
                    continue;
                }

                // This is really only a guard against redownloading the same release over
                // and over when the grabbed and imported qualities do not match, if they do
                // match skip this check.
                if (lastGrabbed.Quality.Equals(imported.Quality))
                {
                    continue;
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
