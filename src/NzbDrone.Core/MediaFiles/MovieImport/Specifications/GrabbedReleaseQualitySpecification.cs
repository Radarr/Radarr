using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class GrabbedReleaseQualitySpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IHistoryService _historyService;

        public GrabbedReleaseQualitySpecification(Logger logger,
            IHistoryService historyService)
        {
            _logger = logger;
            _historyService = historyService;
        }

        public Decision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
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
                    // Log only for info, spec removed due to common webdl/webrip mismatches
                    _logger.Debug("Quality for grabbed release ({0}) does not match the quality of the file ({1})", item.Quality, localMovie.Quality);
                }
            }

            return Decision.Accept();
        }
    }
}
