using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupDuplicateMetadataFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupDuplicateMetadataFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            DeleteDuplicateArtistMetadata();
            DeleteDuplicateAlbumMetadata();
            DeleteDuplicateTrackMetadata();
            DeleteDuplicateTrackImages();
        }

        private void DeleteDuplicateArtistMetadata()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM MetadataFiles
                                     WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE Type = 1
                                         GROUP BY ArtistId, Consumer
                                         HAVING COUNT(ArtistId) > 1
                                     )");
        }

        private void DeleteDuplicateAlbumMetadata()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM MetadataFiles
                                     WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE Type = 6
                                         GROUP BY AlbumId, Consumer
                                         HAVING COUNT(AlbumId) > 1
                                     )");
        }

        private void DeleteDuplicateTrackMetadata()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM MetadataFiles
                                     WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE Type = 2
                                         GROUP BY TrackFileId, Consumer
                                         HAVING COUNT(TrackFileId) > 1
                                     )");
        }

        private void DeleteDuplicateTrackImages()
        {
            var mapper = _database.GetDataMapper();

            mapper.ExecuteNonQuery(@"DELETE FROM MetadataFiles
                                     WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE Type = 5
                                         GROUP BY TrackFileId, Consumer
                                         HAVING COUNT(TrackFileId) > 1
                                     )");
        }
    }
}
