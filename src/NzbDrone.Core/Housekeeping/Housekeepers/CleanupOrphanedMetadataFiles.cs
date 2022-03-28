using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedMetadataFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedMetadataFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            DeleteOrphanedByMovie();
            DeleteOrphanedByMovieFile();
            DeleteWhereMovieFileIsZero();
        }

        private void DeleteOrphanedByMovie()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM ""MetadataFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""MetadataFiles"".""Id"" FROM ""MetadataFiles""
                                     LEFT OUTER JOIN ""Movies""
                                     ON ""MetadataFiles"".""MovieId"" = ""Movies"".""Id""
                                     WHERE ""Movies"".""Id"" IS NULL)");
            }
        }

        private void DeleteOrphanedByMovieFile()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM ""MetadataFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""MetadataFiles"".""Id"" FROM ""MetadataFiles""
                                     LEFT OUTER JOIN ""MovieFiles""
                                     ON ""MetadataFiles"".""MovieFileId"" = ""MovieFiles"".""Id""
                                     WHERE ""MetadataFiles"".""MovieFileId"" > 0
                                     AND ""MovieFiles"".""Id"" IS NULL)");
            }
        }

        private void DeleteWhereMovieFileIsZero()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM ""MetadataFiles""
                                     WHERE ""Id"" IN (
                                     SELECT ""Id"" FROM ""MetadataFiles""
                                     WHERE ""Type"" IN (1, 2)
                                     AND ""MovieFileId"" = 0)");
            }
        }
    }
}
