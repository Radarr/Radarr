using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators
{
    public class AggregateQuality : IAggregateLocalMovie
    {
        private readonly IEnumerable<IAugmentQuality> _augmentQualities;
        private readonly Logger _logger;

        public AggregateQuality(IEnumerable<IAugmentQuality> augmentQualities,
                                Logger logger)
        {
            _augmentQualities = augmentQualities;
            _logger = logger;
        }

        public LocalMovie Aggregate(LocalMovie localMovie, bool otherFiles)
        {
            var augmentedQualities = _augmentQualities.Select(a => a.AugmentQuality(localMovie))
                                                      .Where(a => a != null)
                                                      .OrderBy(a => a.SourceConfidence);

            var source = Source.UNKNOWN;
            var sourceConfidence = Confidence.Default;
            var resolution = Resolution.Unknown;
            var resolutionConfidence = Confidence.Default;
            var revison = new Revision();

            foreach (var augmentedQuality in augmentedQualities)
            {
                if (augmentedQuality.Source > source ||
                    augmentedQuality.SourceConfidence > sourceConfidence && augmentedQuality.Source != Source.UNKNOWN)
                {
                    source = augmentedQuality.Source;
                    sourceConfidence = augmentedQuality.SourceConfidence;
                }

                if (augmentedQuality.Resolution > resolution ||
                    augmentedQuality.ResolutionConfidence > resolutionConfidence && augmentedQuality.Resolution > 0)
                {
                    resolution = augmentedQuality.Resolution;
                    resolutionConfidence = augmentedQuality.ResolutionConfidence;
                }

                if (augmentedQuality.Revision != null && augmentedQuality.Revision > revison)
                {
                    revison = augmentedQuality.Revision;
                }
            }

            _logger.Trace("Finding quality. Source: {0}. Resolution: {1}", source, resolution);

            var quality = new QualityModel(QualityFinder.FindBySourceAndResolution(source, resolution), revison);

            if (resolutionConfidence == Confidence.MediaInfo)
            {
                quality.QualityDetectionSource = QualityDetectionSource.MediaInfo;
            }
            else if (sourceConfidence == Confidence.Fallback || resolutionConfidence == Confidence.Fallback)
            {
                quality.QualityDetectionSource = QualityDetectionSource.Extension;
            }
            else
            {
                quality.QualityDetectionSource = QualityDetectionSource.Name;
            }

            _logger.Debug("Using quality: {0}", quality);

            localMovie.Quality = quality;

            return localMovie;
        }
    }
}
