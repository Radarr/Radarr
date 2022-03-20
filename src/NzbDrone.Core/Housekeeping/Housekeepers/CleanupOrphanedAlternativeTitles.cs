using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupOrphanedAlternativeTitles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupOrphanedAlternativeTitles(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM ""AlternativeTitles""
                                     WHERE ""Id"" IN (
                                     SELECT ""AlternativeTitles"".""Id"" FROM ""AlternativeTitles""
                                     LEFT OUTER JOIN ""MovieMetadata""
                                     ON ""AlternativeTitles"".""MovieMetadataId"" = ""MovieMetadata"".""Id""
                                     WHERE ""MovieMetadata"".""Id"" IS NULL)");
            }
        }
    }
}
