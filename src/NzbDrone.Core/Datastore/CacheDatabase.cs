using System;
using System.Data;

namespace NzbDrone.Core.Datastore
{
    public interface ICacheDatabase : IDatabase
    {
    }

    public class CacheDatabase : ICacheDatabase
    {
        private readonly IDatabase _database;

        public CacheDatabase(IDatabase database)
        {
            _database = database;
        }

        public IDbConnection OpenConnection()
        {
            return _database.OpenConnection();
        }

        public Version Version => _database.Version;

        public int Migration => _database.Migration;

        public void Vacuum()
        {
            _database.Vacuum();
        }
    }
}
