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
                mapper.Execute(@"DELETE FROM Collections
                                     WHERE Id IN (
                                     SELECT Collections.Id FROM Collections
                                     LEFT OUTER JOIN Movies
                                     ON Collections.Id = Movies.CollectionId
                                     WHERE Movies.Id IS NULL)");
            }
        }
    }
}
