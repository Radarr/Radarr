using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.BookImport.Specifications
{
    public class AlreadyImportedSpecification : IImportDecisionEngineSpecification<LocalEdition>
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

        public Decision IsSatisfiedBy(LocalEdition localBookRelease, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                _logger.Debug("No download client information is available, skipping");
                return Decision.Accept();
            }

            var bookRelease = localBookRelease.Edition;

            if ((!bookRelease.BookFiles?.Value?.Any()) ?? true)
            {
                _logger.Debug("Skipping already imported check for book without files");
                return Decision.Accept();
            }

            var bookHistory = _historyService.GetByBook(bookRelease.BookId, null);
            var lastImported = bookHistory.FirstOrDefault(h => h.EventType == HistoryEventType.DownloadImported);
            var lastGrabbed = bookHistory.FirstOrDefault(h => h.EventType == HistoryEventType.Grabbed);

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
