using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality
{
    public class AugmentQualityFromFolder : IAugmentQuality
    {
        public AugmentQualityResult AugmentQuality(LocalMovie localMovie)
        {
            var quality = localMovie.FolderMovieInfo?.Quality;

            if (quality == null)
            {
                return null;
            }

            return new AugmentQualityResult(quality.Quality.Source,
                                            Confidence.Tag,
                                            quality.Quality.Resolution,
                                            Confidence.Tag,
                                            quality.Quality.Modifier,
                                            Confidence.Tag,
                                            quality.Revision);
        }
    }
}
