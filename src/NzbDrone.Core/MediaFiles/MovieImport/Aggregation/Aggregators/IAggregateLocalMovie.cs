using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators
{
    public interface IAggregateLocalMovie
    {
        LocalMovie Aggregate(LocalMovie localMovie, bool otherFiles);
    }
}
