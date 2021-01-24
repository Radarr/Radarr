using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.BookImport.Aggregation.Aggregators
{
    public class AggregateQuality : IAggregate<LocalBook>
    {
        public LocalBook Aggregate(LocalBook localTrack, bool otherFiles)
        {
            var quality = localTrack.FileTrackInfo?.Quality;

            if (quality == null)
            {
                quality = localTrack.FolderTrackInfo?.Quality;
            }

            if (quality == null)
            {
                quality = localTrack.DownloadClientBookInfo?.Quality;
            }

            localTrack.Quality = quality;
            return localTrack;
        }
    }
}
