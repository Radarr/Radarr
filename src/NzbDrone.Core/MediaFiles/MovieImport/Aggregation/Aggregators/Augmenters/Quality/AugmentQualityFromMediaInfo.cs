using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityFromMediaInfo : IAugmentQuality
    {
        public AugmentQualityResult AugmentQuality(LocalMovie localMovie)
        {
            if (localMovie.MediaInfo == null)
            {
                return null;
            }

            var width = localMovie.MediaInfo.Width;

            if (width >= 3200)
            {
                return AugmentQualityResult.ResolutionOnly(Resolution.R2160P, Confidence.MediaInfo);
            }

            if (width >= 1800)
            {
                return AugmentQualityResult.ResolutionOnly(Resolution.R1080P, Confidence.MediaInfo);
            }

            if (width >= 1200)
            {
                return AugmentQualityResult.ResolutionOnly(Resolution.R720P, Confidence.MediaInfo);
            }

            if (width > 0)
            {
                return AugmentQualityResult.ResolutionOnly(Resolution.R480P, Confidence.MediaInfo);
            }

            return null;
        }
    }
}
