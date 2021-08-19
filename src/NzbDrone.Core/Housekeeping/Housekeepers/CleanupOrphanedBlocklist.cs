using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedBlocklist : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedBlocklist(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM Blocklist
                                     WHERE Id IN (
                                     SELECT Blocklist.Id FROM Blocklist
                                     LEFT OUTER JOIN Movies
                                     ON Blocklist.MovieId = Movies.Id
                                     WHERE Movies.Id IS NULL)");
            }
        }
    }
}
