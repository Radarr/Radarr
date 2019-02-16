using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.TrackImport.Aggregation.Aggregators
{
    public class AggregateReleaseGroup : IAggregate<LocalTrack>
    {
        public LocalTrack Aggregate(LocalTrack localTrack, bool otherFiles)
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
