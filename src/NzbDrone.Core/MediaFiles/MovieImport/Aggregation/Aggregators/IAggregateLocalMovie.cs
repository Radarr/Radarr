using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators
{
    public interface IAggregateLocalMovie
    {
        int Order { get; }

        LocalMovie Aggregate(LocalMovie localMovie, DownloadClientItem downloadClientItem);
    }
}
