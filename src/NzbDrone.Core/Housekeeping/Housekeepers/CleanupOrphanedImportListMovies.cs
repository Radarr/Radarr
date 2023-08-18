using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedImportListMovies : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedImportListMovies(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();

            mapper.Execute(@"DELETE FROM ""ImportListMovies""
                                     WHERE ""Id"" IN (
                                     SELECT ""ImportListMovies"".""Id"" FROM ""ImportListMovies""
                                     LEFT OUTER JOIN ""ImportLists""
                                     ON ""ImportListMovies"".""ListId"" = ""ImportLists"".""Id""
                                     WHERE ""ImportLists"".""Id"" IS NULL)");
        }
    }
}
