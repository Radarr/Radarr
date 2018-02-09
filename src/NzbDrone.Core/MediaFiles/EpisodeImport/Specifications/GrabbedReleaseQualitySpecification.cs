using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class GrabbedReleaseQualitySpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IHistoryService _historyService;

        public GrabbedReleaseQualitySpecification(Logger logger, IHistoryService historyService)
        {
            _logger = logger;
            _historyService = historyService;
        }

        public Decision IsSatisfiedBy(LocalEpisode localEpisode)
        {
            var qualityComparer = new QualityModelComparer(localEpisode.Series.Profile);
            if (localEpisode.Episodes.Any(e => e.EpisodeFileId != 0 && qualityComparer.Compare(e.EpisodeFile.Value.Quality, localEpisode.Quality) > 0))
            {
                _logger.Debug("This file isn't an upgrade for all episodes. Skipping {0}", localEpisode.Path);
                return Decision.Reject("Not an upgrade for existing episode file(s)");
            }

            return Decision.Accept();
        }

        public Decision IsSatisfiedBy(LocalMovie localEpisode, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                _logger.Debug("No download client item provided, skipping.");
                return Decision.Accept();
            }

            var grabbedHistory = _historyService.FindByDownloadId(downloadClientItem.DownloadId)
                .Where(h => h.EventType == HistoryEventType.Grabbed)
                .ToList();

            if (grabbedHistory.Empty())
            {
                _logger.Debug("No grabbed history for this download client item");
                return Decision.Accept();
            }

            var parsedReleaseName = Parser.Parser.ParseMovieTitle(grabbedHistory.First().SourceTitle, false);

            foreach (var item in grabbedHistory)
            {
                if (item.Quality.Quality != Quality.Unknown && item.Quality != localEpisode.Quality)
                {
                    _logger.Debug("Quality for grabbed release ({0}) does not match the quality of the file ({1})", item.Quality, localEpisode.Quality);
                    return Decision.Reject("File quality does not match quality of the grabbed release");
                }
            }

            return Decision.Accept();
        }
    }
}
