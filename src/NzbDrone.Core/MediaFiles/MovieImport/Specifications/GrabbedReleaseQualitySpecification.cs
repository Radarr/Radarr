using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class GrabbedReleaseQualitySpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IHistoryService _historyService;
        private readonly IParsingService _parsingService;

        public GrabbedReleaseQualitySpecification(Logger logger,
            IHistoryService historyService,
            IParsingService parsingService)
        {
            _logger = logger;
            _historyService = historyService;
            _parsingService = parsingService;
        }

        public Decision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                _logger.Debug("No download client item provided, skipping.");
                return Decision.Accept();
            }

            var grabbedHistory = _historyService.FindByDownloadId(downloadClientItem.DownloadId)
                .Where(h => h.EventType == MovieHistoryEventType.Grabbed)
                .ToList();

            if (grabbedHistory.Empty())
            {
                _logger.Debug("No grabbed history for this download client item");
                return Decision.Accept();
            }

            foreach (var item in grabbedHistory)
            {
                if (item.Quality.Quality != Quality.Unknown && item.Quality != localMovie.Quality)
                {
                    _logger.Debug("Quality for grabbed release ({0}) does not match the quality of the file ({1})", item.Quality, localMovie.Quality);
                    return Decision.Reject("File quality does not match quality of the grabbed release");
                }
            }

            return Decision.Accept();
        }
    }
}
