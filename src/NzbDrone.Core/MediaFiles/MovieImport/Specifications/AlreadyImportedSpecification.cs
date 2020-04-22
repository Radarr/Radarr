using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class AlreadyImportedSpecification : IImportDecisionEngineSpecification
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

        public Decision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                _logger.Debug("No download client information is available, skipping");
                return Decision.Accept();
            }

            var movie = localMovie.Movie;

            if (!movie.HasFile)
            {
                _logger.Debug("Skipping already imported check for movie without file");
                return Decision.Accept();
            }

            var movieImportedHistory = _historyService.GetByMovieId(movie.Id, null);
            var lastImported = movieImportedHistory.FirstOrDefault(h => h.EventType == HistoryEventType.DownloadFolderImported);
            var lastGrabbed = movieImportedHistory.FirstOrDefault(h => h.EventType == HistoryEventType.Grabbed);

            if (lastImported == null)
            {
                return Decision.Accept();
            }

            // If the release was grabbed again after importing don't reject it
            if (lastGrabbed != null && lastGrabbed.Date.After(lastImported.Date))
            {
                return Decision.Accept();
            }

            if (lastImported.DownloadId == downloadClientItem.DownloadId)
            {
                _logger.Debug("Movie file previously imported at {0}", lastImported.Date);
                return Decision.Reject("Movie file already imported at {0}", lastImported.Date);
            }

            return Decision.Accept();
        }
    }
}
