using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles.TrackImport;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class AlreadyImportedSpecification : IImportDecisionEngineSpecification<LocalAlbumRelease>
    {
        private readonly IHistoryService _historyService;
        private readonly Logger _logger;

        public AlreadyImportedSpecification(IHistoryService historyService,
                                            Logger logger)
        {
            _historyService = historyService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;

        public Decision IsSatisfiedBy(LocalAlbumRelease localAlbumRelease, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                _logger.Debug("No download client information is available, skipping");
                return Decision.Accept();
            }

            var albumRelease = localAlbumRelease.Book;

            if ((!albumRelease?.BookFiles?.Value?.Any()) ?? true)
            {
                _logger.Debug("Skipping already imported check for book without files");
                return Decision.Accept();
            }

            var albumHistory = _historyService.GetByAlbum(albumRelease.Id, null);
            var lastImported = albumHistory.FirstOrDefault(h => h.EventType == HistoryEventType.DownloadImported);
            var lastGrabbed = albumHistory.FirstOrDefault(h => h.EventType == HistoryEventType.Grabbed);

            if (lastImported == null)
            {
                return Decision.Accept();
            }

            if (lastGrabbed != null && lastGrabbed.Date.After(lastImported.Date))
            {
                return Decision.Accept();
            }

            if (lastImported.DownloadId == downloadClientItem.DownloadId)
            {
                _logger.Debug("Book previously imported at {0}", lastImported.Date);
                return Decision.Reject("Book already imported at {0}", lastImported.Date);
            }

            return Decision.Accept();
        }
    }
}
