using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedBooks : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedBooks(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM Books
                                     WHERE Id IN (
                                     SELECT Books.Id FROM Books
                                     LEFT OUTER JOIN Authors
                                     ON Books.AuthorMetadataId = Authors.AuthorMetadataId
                                     WHERE Authors.Id IS NULL)");
            }
        }
    }
}
