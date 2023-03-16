using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Specifications
{
    public class MatchesGrabSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IParsingService _parsingService;
        private readonly IHistoryService _historyService;

        public MatchesGrabSpecification(IParsingService parsingService, IHistoryService historyService, Logger logger)
        {
            _logger = logger;
            _parsingService = parsingService;
            _historyService = historyService;
        }

        public Decision IsSatisfiedBy(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (localMovie.ExistingFile)
            {
                return Decision.Accept();
            }

            if (downloadClientItem == null)
            {
                return Decision.Accept();
            }

            var grabbedHistory = _historyService.FindByDownloadId(downloadClientItem.DownloadId)
                .Where(h => h.EventType == MovieHistoryEventType.Grabbed)
                .ToList();

            if (grabbedHistory.Empty())
            {
                return Decision.Accept();
            }

            if (grabbedHistory.All(o => o.MovieId != localMovie.Movie.Id))
            {
                _logger.Debug("Unexpected movie(s) in file: {0}", localMovie.Movie.ToString());

                return Decision.Reject("Movie {0} was not found in the grabbed release: {1}", localMovie.Movie.ToString(), grabbedHistory.First().SourceTitle);
            }

            return Decision.Accept();
        }
    }
}
