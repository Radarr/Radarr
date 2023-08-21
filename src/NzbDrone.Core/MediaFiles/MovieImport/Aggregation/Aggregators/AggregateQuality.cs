using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators
{
    public class AggregateQuality : IAggregateLocalMovie
    {
        private readonly List<IAugmentQuality> _augmentQualities;
        private readonly Logger _logger;

        public AggregateQuality(IEnumerable<IAugmentQuality> augmentQualities,
                                Logger logger)
        {
            _augmentQualities = augmentQualities.OrderBy(a => a.Order).ToList();
            _logger = logger;
        }

        public LocalMovie Aggregate(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            var source = QualitySource.UNKNOWN;
            var sourceConfidence = Confidence.Default;
            var resolution = 0;
            var resolutionConfidence = Confidence.Default;
            var modifier = Modifier.NONE;
            var modifierConfidence = Confidence.Default;
            var revision = new Revision(1);
            var revisionConfidence = Confidence.Default;

            foreach (var augmentQuality in _augmentQualities)
            {
                var augmentedQuality = augmentQuality.AugmentQuality(localMovie, downloadClientItem);
                if (augmentedQuality == null)
                {
                    continue;
                }

                _logger.Trace("Considering Source {0} ({1}) Resolution {2} ({3}) Revision {4} from {5}", augmentedQuality.Source, augmentedQuality.SourceConfidence, augmentedQuality.Resolution, augmentedQuality.ResolutionConfidence, augmentedQuality.Revision, augmentQuality.Name);

                if (source == QualitySource.UNKNOWN ||
                    (augmentedQuality.SourceConfidence > sourceConfidence && augmentedQuality.Source != QualitySource.UNKNOWN))
                {
                    source = augmentedQuality.Source;
                    sourceConfidence = augmentedQuality.SourceConfidence;
                }

                if (resolution == 0 ||
                    (augmentedQuality.ResolutionConfidence > resolutionConfidence && augmentedQuality.Resolution > 0))
                {
                    resolution = augmentedQuality.Resolution;
                    resolutionConfidence = augmentedQuality.ResolutionConfidence;
                }

                if (augmentedQuality.Modifier > modifier ||
                    (augmentedQuality.ModifierConfidence > modifierConfidence && augmentedQuality.Modifier != Modifier.NONE))
                {
                    modifier = augmentedQuality.Modifier;
                    modifierConfidence = augmentedQuality.ModifierConfidence;
                }

                if (augmentedQuality.Revision != null)
                {
                    // Update the revision and confidence if it is higher than the current confidence,
                    // this will allow explicitly detected v0 to override the v1 default.
                    if (augmentedQuality.RevisionConfidence > revisionConfidence)
                    {
                        revision = augmentedQuality.Revision;
                        revisionConfidence = augmentedQuality.RevisionConfidence;
                    }

                    // Update the revision and confidence if it is the same confidence and the revision is higher,
                    // this will allow the best revision to be used in the event there is a disagreement.
                    else if (augmentedQuality.RevisionConfidence == revisionConfidence &&
                             augmentedQuality.Revision > revision)
                    {
                        revision = augmentedQuality.Revision;
                        revisionConfidence = augmentedQuality.RevisionConfidence;
                    }
                }
            }

            _logger.Trace("Selected Source {0} ({1}) Resolution {2} ({3}) Revision {4}", source, sourceConfidence, resolution, resolutionConfidence, revision);

            var quality = new QualityModel(QualityFinder.FindBySourceAndResolution(source, resolution, modifier), revision);

            if (resolutionConfidence == Confidence.MediaInfo)
            {
                quality.ResolutionDetectionSource = QualityDetectionSource.MediaInfo;
            }
            else if (resolutionConfidence == Confidence.Fallback)
            {
                quality.ResolutionDetectionSource = QualityDetectionSource.Extension;
            }
            else
            {
                quality.ResolutionDetectionSource = QualityDetectionSource.Name;
            }

            if (sourceConfidence == Confidence.Fallback)
            {
                quality.SourceDetectionSource = QualityDetectionSource.Extension;
            }
            else
            {
                quality.SourceDetectionSource = QualityDetectionSource.Name;
            }

            quality.RevisionDetectionSource = revisionConfidence == Confidence.Tag ? QualityDetectionSource.Name : QualityDetectionSource.Unknown;

            _logger.Debug("Using quality: {0}", quality);

            localMovie.Quality = quality;

            return localMovie;
        }
    }
}
