using NzbDrone.Core.Download;
using NzbDrone.Core.Download.History;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityFromReleaseName : IAugmentQuality
    {
        public int Order => 5;
        public string Name => "ReleaseName";

        private readonly IDownloadHistoryService _downloadHistoryService;

        public AugmentQualityFromReleaseName(IDownloadHistoryService downloadHistoryService)
        {
            _downloadHistoryService = downloadHistoryService;
        }

        public AugmentQualityResult AugmentQuality(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            // Don't try to augment if we can't lookup the grabbed history by downloadId
            if (downloadClientItem == null)
            {
                return null;
            }

            var history = _downloadHistoryService.GetLatestGrab(downloadClientItem.DownloadId);
            if (history == null)
            {
                return null;
            }

            var historyQuality = QualityParser.ParseQuality(history.SourceTitle);
            var sourceConfidence = historyQuality.SourceDetectionSource == QualityDetectionSource.Name
                ? Confidence.Tag
                : Confidence.Fallback;
            var resolutionConfidence = historyQuality.ResolutionDetectionSource == QualityDetectionSource.Name
                ? Confidence.Tag
                : Confidence.Fallback;
            var modifierConfidence = historyQuality.ModifierDetectionSource == QualityDetectionSource.Name
                ? Confidence.Tag
                : Confidence.Fallback;

            return new AugmentQualityResult(historyQuality.Quality.Source, sourceConfidence, historyQuality.Quality.Resolution, resolutionConfidence, historyQuality.Quality.Modifier, modifierConfidence, historyQuality.Revision);
        }
    }
}
