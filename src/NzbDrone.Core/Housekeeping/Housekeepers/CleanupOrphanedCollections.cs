using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedCollections : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedCollections(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM ""Collections""
                                     WHERE ""TmdbId"" IN (
                                     SELECT ""Collections"".""TmdbId"" FROM ""Collections""
                                     LEFT OUTER JOIN ""MovieMetadata""
                                     ON ""Collections"".""TmdbId"" = ""MovieMetadata"".""CollectionTmdbId""
                                     WHERE ""MovieMetadata"".""Id"" IS NULL)");
            }
        }
    }
}
