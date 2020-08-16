using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators
{
    public class AggregateReleaseGroup : IAggregateLocalMovie
    {
        public LocalMovie Aggregate(LocalMovie localMovie, DownloadClientItem downloadClientItem, bool otherFiles)
        {
            var releaseGroup = localMovie.DownloadClientMovieInfo?.ReleaseGroup;

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = localMovie.FolderMovieInfo?.ReleaseGroup;
            }

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = localMovie.FileMovieInfo?.ReleaseGroup;
            }

            localMovie.ReleaseGroup = releaseGroup;

            return localMovie;
        }
    }
}
