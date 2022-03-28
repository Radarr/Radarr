using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class CleanupUnusedTags : IHousekeepingTask
    {
        private readonly IMainDatabase _database;

        public CleanupUnusedTags(IMainDatabase database)
        {
            _database = database;
        }

        public void Clean()
        {
            var mapper = _database.OpenConnection();

            var usedTags = new[] { "Movies", "Notifications", "DelayProfiles", "Restrictions", "ImportLists", "Indexers" }
                .SelectMany(v => GetUsedTags(v, mapper))
                .Distinct()
                .ToList();

            if (usedTags.Any())
            {
                var usedTagsList = usedTags.Select(d => d.ToString()).Join(",");

                if (_database.DatabaseType == DatabaseType.PostgreSQL)
                {
                    mapper.Execute($"DELETE FROM \"Tags\" WHERE NOT \"Id\" = ANY (\'{{{usedTagsList}}}\'::int[])");
                }
                else
                {
                    mapper.Execute($"DELETE FROM \"Tags\" WHERE NOT \"Id\" IN ({usedTagsList})");
                }
            }
            else
            {
                mapper.Execute("DELETE FROM \"Tags\"");
            }
        }

        private int[] GetUsedTags(string table, IDbConnection mapper)
        {
            return mapper.Query<List<int>>($"SELECT DISTINCT \"Tags\" FROM \"{table}\" WHERE NOT \"Tags\" = '[]' AND NOT \"Tags\" IS NULL")
                .SelectMany(x => x)
                .Distinct()
                .ToArray();
        }
    }
}
