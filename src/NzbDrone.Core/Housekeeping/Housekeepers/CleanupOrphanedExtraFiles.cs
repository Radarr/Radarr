using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedExtraFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedExtraFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            DeleteOrphanedByMovie();
            DeleteOrphanedByMovieFile();
        }

        private void DeleteOrphanedByMovie()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM ""ExtraFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""ExtraFiles"".""Id"" FROM ""ExtraFiles""
                                     LEFT OUTER JOIN ""Movies""
                                     ON ""ExtraFiles"".""MovieId"" = ""Movies"".""Id""
                                     WHERE ""Movies"".""Id"" IS NULL)");
            }
        }

        private void DeleteOrphanedByMovieFile()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM ""ExtraFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""ExtraFiles"".""Id"" FROM ""ExtraFiles""
                                     LEFT OUTER JOIN ""MovieFiles""
                                     ON ""ExtraFiles"".""MovieFileId"" = ""MovieFiles"".""Id""
                                     WHERE ""ExtraFiles"".""MovieFileId"" > 0
                                     AND ""MovieFiles"".""Id"" IS NULL)");
            }
        }
    }
}
