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
            DeleteDuplicateBookMetadata();
            DeleteDuplicateBookImages();
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

        private void DeleteDuplicateBookMetadata()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM MetadataFiles
                                         WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE Type = 2
                                         GROUP BY BookFileId, Consumer
                                         HAVING COUNT(BookFileId) > 1
                                     )");
            }
        }

        private void DeleteDuplicateBookImages()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM MetadataFiles
                                         WHERE Id IN (
                                         SELECT Id FROM MetadataFiles
                                         WHERE Type = 4
                                         GROUP BY BookFileId, Consumer
                                         HAVING COUNT(BookFileId) > 1
                                     )");
            }
        }
    }
}
