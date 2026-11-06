using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedMovieVersions : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedMovieVersions(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"DELETE FROM ""MovieVersions""
                             WHERE ""Id"" IN (
                             SELECT ""MovieVersions"".""Id"" FROM ""MovieVersions""
                             LEFT OUTER JOIN ""Movies""
                             ON ""MovieVersions"".""MovieId"" = ""Movies"".""Id""
                             WHERE ""Movies"".""Id"" IS NULL)");
        }
    }
}
