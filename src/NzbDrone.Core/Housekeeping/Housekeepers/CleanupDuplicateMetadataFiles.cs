using Dapper;
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
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM MetadataFiles
                                     WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE Type = 1
                                         GROUP BY AuthorId, Consumer
                                         HAVING COUNT(AuthorId) > 1
                                     )");
            }
        }

        private void DeleteDuplicateAlbumMetadata()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM MetadataFiles
                                         WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE Type = 6
                                         GROUP BY BookId, Consumer
                                         HAVING COUNT(BookId) > 1
                                     )");
            }
        }

        private void DeleteDuplicateTrackMetadata()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM MetadataFiles
                                         WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE Type = 2
                                         GROUP BY TrackFileId, Consumer
                                         HAVING COUNT(TrackFileId) > 1
                                     )");
            }
        }

        private void DeleteDuplicateTrackImages()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM MetadataFiles
                                         WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE Type = 5
                                         GROUP BY TrackFileId, Consumer
                                         HAVING COUNT(TrackFileId) > 1
                                     )");
            }
        }
    }
}
