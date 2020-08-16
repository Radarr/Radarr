using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

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

            if (width >= 3200 || height >= 2100)
            {
                _logger.Trace("Resolution {0}x{1} considered 2160p", width, height);
                return AugmentQualityResult.ResolutionOnly((int)Resolution.R2160p, Confidence.MediaInfo);
            }

            if (width >= 1800 || height >= 1000)
            {
                _logger.Trace("Resolution {0}x{1} considered 1080p", width, height);
                return AugmentQualityResult.ResolutionOnly((int)Resolution.R1080p, Confidence.MediaInfo);
            }

            if (width >= 1200 || height >= 700)
            {
                _logger.Trace("Resolution {0}x{1} considered 720p", width, height);
                return AugmentQualityResult.ResolutionOnly((int)Resolution.R720p, Confidence.MediaInfo);
            }

            if (width > 0 && height > 0)
            {
                _logger.Trace("Resolution {0}x{1} considered 480p", width, height);
                return AugmentQualityResult.ResolutionOnly((int)Resolution.R480p, Confidence.MediaInfo);
            }

            _logger.Trace("Resolution {0}x{1}", width, height);

            return null;
        }
    }
}
