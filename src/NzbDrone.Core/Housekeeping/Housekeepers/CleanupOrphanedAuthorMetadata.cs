using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedAuthorMetadata : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedAuthorMetadata(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM AuthorMetadata
                                     WHERE Id IN (
                                     SELECT AuthorMetadata.Id FROM AuthorMetadata
                                     LEFT OUTER JOIN Books ON Books.AuthorMetadataId = AuthorMetadata.Id
                                     LEFT OUTER JOIN Authors ON Authors.AuthorMetadataId = AuthorMetadata.Id
                                     WHERE Books.Id IS NULL AND Authors.Id IS NULL)");
            }
        }
    }
}
