using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedMovies : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedMovies(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""Movies""
                             WHERE ""Id"" IN (
                             SELECT ""Movies"".""Id"" FROM ""Movies""
                             LEFT OUTER JOIN ""MovieMetadata"" ON ""Movies"".""MovieMetadataId"" = ""MovieMetadata"".""Id""
                             WHERE ""MovieMetadata"".""Id"" IS NULL)");
        }
    }
}
