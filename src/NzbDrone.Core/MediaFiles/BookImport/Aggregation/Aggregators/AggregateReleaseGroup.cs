using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.BookImport.Aggregation.Aggregators
{
    public class AggregateReleaseGroup : IAggregate<LocalBook>
    {
        public LocalBook Aggregate(LocalBook localTrack, bool otherFiles)
        {
            var releaseGroup = localTrack.DownloadClientAlbumInfo?.ReleaseGroup;

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = localTrack.FolderTrackInfo?.ReleaseGroup;
            }

            if (releaseGroup.IsNullOrWhiteSpace())
            {
                releaseGroup = localTrack.FileTrackInfo?.ReleaseGroup;
            }

            localTrack.ReleaseGroup = releaseGroup;

            return localTrack;
        }
    }
}
