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
            var lastImported = movieImportedHistory.FirstOrDefault(h =>
                h.DownloadId == downloadClientItem.DownloadId &&
                h.EventType == MovieHistoryEventType.DownloadFolderImported);
            var lastGrabbed = movieImportedHistory.FirstOrDefault(h =>
                h.DownloadId == downloadClientItem.DownloadId && h.EventType == MovieHistoryEventType.Grabbed);

            if (lastImported == null)
            {
                _logger.Trace("Movie file has not been imported");
                return Decision.Accept();
            }

            if (lastGrabbed != null)
            {
                // If the release was grabbed again after importing don't reject it
                if (lastGrabbed.Date.After(lastImported.Date))
                {
                    _logger.Trace("Movie file was grabbed again after importing");
                    return Decision.Accept();
                }

                // If the release was imported after the last grab reject it
                if (lastImported.Date.After(lastGrabbed.Date))
                {
                    _logger.Debug("Movie file previously imported at {0}", lastImported.Date);
                    return Decision.Reject("Movie file already imported at {0}", lastImported.Date.ToLocalTime());
                }
            }
            else
            {
                _logger.Debug("Movie file previously imported at {0}", lastImported.Date);
                return Decision.Reject("Movie file already imported at {0}", lastImported.Date.ToLocalTime());
            }

            return Decision.Accept();
        }
    }
}
