using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality
{
    public interface IAugmentQuality
    {
        AugmentQualityResult AugmentQuality(LocalMovie localMovie);
    }
}
