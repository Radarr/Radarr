using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupDuplicateMovieTranslations : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupDuplicateMovieTranslations(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using var mapper = _database.OpenConnection();

            mapper.Execute(@"DELETE FROM ""MovieTranslations""
            WHERE ""Id"" IN (
                SELECT MAX(""Id"") FROM ""MovieTranslations""
                GROUP BY ""MovieMetadataId"", ""Language""
                HAVING COUNT(""Id"") > 1
            )");
        }
    }
}
