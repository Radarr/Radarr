using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators
{
    public interface IAggregateLocalMovie
    {
        LocalMovie Aggregate(LocalMovie localMovie, DownloadClientItem downloadClientItem, bool otherFiles);
    }
}
