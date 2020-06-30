using Dapper;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class TrimHttpCache : IHousekeepingTask
    {
        private readonly ICacheDatabase _database;

        public TrimHttpCache(ICacheDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            using (var mapper = _database.OpenConnection())
            {
                mapper.Execute(@"DELETE FROM HttpResponse WHERE Expiry < date('now')");
            }
        }
    }
}
