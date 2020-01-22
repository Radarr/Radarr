using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityFromFileName : IAugmentQuality
    {
        public AugmentQualityResult AugmentQuality(LocalMovie localMovie)
        {
            var quality = localMovie.FileMovieInfo?.Quality;

            if (quality == null)
            {
                return null;
            }

            var confidence = quality.QualityDetectionSource == QualityDetectionSource.Extension
                ? Confidence.Fallback
                : Confidence.Tag;

            return new AugmentQualityResult(quality.Quality.Source,
                                            confidence,
                                            quality.Quality.Resolution,
                                            confidence,
                                            quality.Quality.Modifier,
                                            confidence,
                                            quality.Revision);
        }
    }
}
