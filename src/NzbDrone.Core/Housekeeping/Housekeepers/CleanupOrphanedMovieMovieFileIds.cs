using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedMovieMovieFileIds : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedMovieMovieFileIds(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"UPDATE ""Movies""
                                 SET ""MovieFileId"" = 0
                                 WHERE ""Id"" IN (
                                 SELECT ""Movies"".""Id"" FROM ""Movies""
                                 LEFT OUTER JOIN ""MovieFiles""
                                 ON ""Movies"".""MovieFileId"" = ""MovieFiles"".""Id""
                                 WHERE ""MovieFiles"".""Id"" IS NULL)");
            }
        }
    }
}
