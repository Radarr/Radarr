using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedBookFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedBookFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using (var mapper = _database.OpenConnection())
            {
                // Unlink where track no longer exists
                mapper.Execute(@"UPDATE BookFiles
                                     SET EditionId = 0
                                     WHERE Id IN (
                                     SELECT BookFiles.Id FROM BookFiles
                                     LEFT OUTER JOIN Editions
                                     ON BookFiles.EditionId = Editions.Id
                                     WHERE Editions.Id IS NULL)");
            }
        }
    }
}
