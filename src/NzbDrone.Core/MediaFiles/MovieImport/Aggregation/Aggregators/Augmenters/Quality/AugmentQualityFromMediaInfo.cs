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
                return AugmentQualityResult.ResolutionOnly((int)Resolution.R2160p, Confidence.MediaInfo);
            }

            if (width >= 1800)
            {
                return AugmentQualityResult.ResolutionOnly((int)Resolution.R1080p, Confidence.MediaInfo);
            }

            if (width >= 1200)
            {
                return AugmentQualityResult.ResolutionOnly((int)Resolution.R720p, Confidence.MediaInfo);
            }

            if (width > 0)
            {
                return AugmentQualityResult.ResolutionOnly((int)Resolution.R480p, Confidence.MediaInfo);
            }

            return null;
        }
    }
}
