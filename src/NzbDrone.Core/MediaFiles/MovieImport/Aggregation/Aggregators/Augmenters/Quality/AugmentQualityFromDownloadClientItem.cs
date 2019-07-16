using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityFromDownloadClientItem : IAugmentQuality
    {
        public AugmentQualityResult AugmentQuality(LocalMovie localMovie)
        {
            var quality = localMovie.DownloadClientMovieInfo?.Quality;

            if (quality == null)
            {
                return null;
            }

            return new AugmentQualityResult(quality.Quality.Source,
                                            Confidence.Tag,
                                            quality.Quality.Resolution,
                                            Confidence.Tag,
                                            quality.Revision);
        }
    }
}
