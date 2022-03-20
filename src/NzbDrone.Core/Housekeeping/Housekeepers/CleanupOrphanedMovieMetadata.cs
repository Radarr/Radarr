using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedMovieMetadata : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedMovieMetadata(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM ""MovieMetadata""
                                     WHERE ""Id"" IN (
                                     SELECT ""MovieMetadata"".""Id"" FROM ""MovieMetadata""
                                     LEFT OUTER JOIN ""Movies"" ON ""Movies"".""MovieMetadataId"" = ""MovieMetadata"".""Id""
                                     LEFT OUTER JOIN ""ImportListMovies"" ON ""ImportListMovies"".""MovieMetadataId"" = ""MovieMetadata"".""Id""
                                     WHERE ""Movies"".""Id"" IS NULL AND ""ImportListMovies"".""Id"" IS NULL)");
            }
        }
    }
}
