using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedHistoryItems : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedHistoryItems(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            CleanupOrphanedByAuthor();
            CleanupOrphanedByBook();
        }

        private void CleanupOrphanedByAuthor()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM History
                                     WHERE Id IN (
                                     SELECT History.Id FROM History
                                     LEFT OUTER JOIN Authors
                                     ON History.AuthorId = Authors.Id
                                     WHERE Authors.Id IS NULL)");
            }
        }

        private void CleanupOrphanedByBook()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM History
                                     WHERE Id IN (
                                     SELECT History.Id FROM History
                                     LEFT OUTER JOIN Books
                                     ON History.BookId = Books.Id
                                     WHERE Books.Id IS NULL)");
            }
        }
    }
}
