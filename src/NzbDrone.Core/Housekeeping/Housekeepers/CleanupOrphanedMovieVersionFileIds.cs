using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedMovieVersionFileIds : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedMovieVersionFileIds(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();
            mapper.Execute(@"UPDATE ""MovieVersions""
                             SET ""MovieFileId"" = 0
                             WHERE ""Id"" IN (
                             SELECT ""MovieVersions"".""Id"" FROM ""MovieVersions""
                             LEFT OUTER JOIN ""MovieFiles""
                             ON ""MovieVersions"".""MovieFileId"" = ""MovieFiles"".""Id""
                             WHERE ""MovieVersions"".""MovieFileId"" > 0
                             AND ""MovieFiles"".""Id"" IS NULL)");
        }
    }
}
