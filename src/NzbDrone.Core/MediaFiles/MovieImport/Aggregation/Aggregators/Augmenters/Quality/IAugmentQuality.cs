using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators.Augmenters.Quality
{
    public interface IAugmentQuality
    {
        int Order { get; }
        string Name { get; }

        AugmentQualityResult AugmentQuality(LocalMovie localMovie, DownloadClientItem downloadClientItem);
    }
}
