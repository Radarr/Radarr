using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class DifferentQualitySpecification : IImportDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly Logger _logger;

        public DifferentQualitySpecification(IHistoryService historyService, Logger logger)
        {
            _historyService = historyService;
            _logger = logger;
        }

        public Decision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                _logger.Debug("No download client item, skipping");
                return Decision.Accept();
            }

            var grabbedMovieHistory = _historyService.FindByDownloadId(downloadClientItem.DownloadId)
                                                       .OrderByDescending(h => h.Date)
                                                       .FirstOrDefault(h => h.EventType == MovieHistoryEventType.Grabbed);

            if (grabbedMovieHistory == null)
            {
                _logger.Debug("No grabbed history for this download item, skipping");
                return Decision.Accept();
            }

            var qualityComparer = new QualityModelComparer(localMovie.Movie.Profile);
            var qualityCompare = qualityComparer.Compare(localMovie.Quality, grabbedMovieHistory.Quality);

            if (qualityCompare != 0)
            {
                _logger.Debug("Quality of file ({0}) does not match quality of grabbed history ({1})", localMovie.Quality, grabbedMovieHistory.Quality);
                return Decision.Reject("Not an upgrade for existing movie file(s)");
            }

            return Decision.Accept();
        }
    }
}
