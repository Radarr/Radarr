using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupDuplicateMetadataFiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupDuplicateMetadataFiles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            DeleteDuplicateMovieMetadata();
            DeleteDuplicateMovieFileMetadata();
        }

        private void DeleteDuplicateMovieMetadata()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM ""MetadataFiles""
                                     WHERE ""Id"" IN (
                                         SELECT MIN(""Id"") FROM ""MetadataFiles""
                                         WHERE ""Type"" = 1
                                         GROUP BY ""MovieId"", ""Consumer""
                                         HAVING COUNT(""MovieId"") > 1
                                     )");
            }
        }

        private void DeleteDuplicateMovieFileMetadata()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM ""MetadataFiles""
                                     WHERE ""Id"" IN (
                                         SELECT MIN(""Id"") FROM ""MetadataFiles""
                                         WHERE ""Type"" = 1
                                         GROUP BY ""MovieFileId"", ""Consumer""
                                         HAVING COUNT(""MovieFileId"") > 1
                                     )");
            }
        }
    }
}
