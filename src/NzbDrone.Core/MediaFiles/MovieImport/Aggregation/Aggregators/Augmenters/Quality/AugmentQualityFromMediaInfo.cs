using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityFromMediaInfo : IAugmentQuality
    {
        private readonly Logger _logger;

        public int Order => 4;
        public string Name => "MediaInfo";

        public AugmentQualityFromMediaInfo(Logger logger)
        {
            _logger = logger;
        }

        public AugmentQualityResult AugmentQuality(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (localMovie.MediaInfo == null)
            {
                return null;
            }

            var width = localMovie.MediaInfo.Width;
            var height = localMovie.MediaInfo.Height;
            var source = QualitySource.UNKNOWN;
            var sourceConfidence = Confidence.Default;
            var title = localMovie.MediaInfo.Title;

            if (title.IsNotNullOrWhiteSpace())
            {
                var parsedQuality = QualityParser.ParseQualityName(title.Trim());

                // Only use the quality if it's not unknown and the source is from the name (which is MediaInfo's title in this case)
                if (parsedQuality.Quality.Source != QualitySource.UNKNOWN &&
                    parsedQuality.SourceDetectionSource == QualityDetectionSource.Name)
                {
                    source = parsedQuality.Quality.Source;
                    sourceConfidence = Confidence.MediaInfo;
                }
            }

            if (width >= 3200 || height >= 2100)
            {
                _logger.Trace("Resolution {0}x{1} considered 2160p", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, (int)Resolution.R2160p, Confidence.MediaInfo);
            }

            if (width >= 1800 || height >= 1000)
            {
                _logger.Trace("Resolution {0}x{1} considered 1080p", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, (int)Resolution.R1080p, Confidence.MediaInfo);
            }

            if (width >= 1200 || height >= 700)
            {
                _logger.Trace("Resolution {0}x{1} considered 720p", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, (int)Resolution.R720p, Confidence.MediaInfo);
            }

            if (width >= 1000 || height >= 560)
            {
                _logger.Trace("Resolution {0}x{1} considered 576p", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, (int)Resolution.R576p, Confidence.MediaInfo);
            }

            if (width > 0 && height > 0)
            {
                _logger.Trace("Resolution {0}x{1} considered 480p", width, height);
                return AugmentQualityResult.SourceAndResolutionOnly(source, sourceConfidence, (int)Resolution.R480p, Confidence.MediaInfo);
            }

            _logger.Trace("Resolution {0}x{1}", width, height);

            return null;
        }
    }
}
