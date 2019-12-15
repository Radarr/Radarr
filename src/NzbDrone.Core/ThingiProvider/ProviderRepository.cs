using System.Collections.Generic;
using System.Text.Json;
using Dapper;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ThingiProvider
{
    public class ProviderRepository<TProviderDefinition> : BasicRepository<TProviderDefinition>, IProviderRepository<TProviderDefinition>
        where TProviderDefinition : ProviderDefinition, new()
    {
        protected ProviderRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        protected override IEnumerable<TProviderDefinition> GetResults(SqlBuilder.Template sql)
        {
            var results = new List<TProviderDefinition>();

            using (var conn = _database.OpenConnection())
            using (var reader = conn.ExecuteReader(sql.RawSql, sql.Parameters))
            {
                var parser = reader.GetRowParser<TProviderDefinition>(typeof(TProviderDefinition));
                var settingsIndex = reader.GetOrdinal("Settings");
                var serializerSettings = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                while (reader.Read())
                {
                    var body = reader.IsDBNull(settingsIndex) ? null : reader.GetString(settingsIndex);
                    var item = parser(reader);
                    var impType = typeof(IProviderConfig).Assembly.FindTypeByName(item.ConfigContract);

                    if (body.IsNullOrWhiteSpace())
                    {
                        item.Settings = NullConfig.Instance;
                    }
                    else
                    {
                        item.Settings = (IProviderConfig) JsonSerializer.Deserialize(body, impType, serializerSettings);
                    }

                    results.Add(item);
                }
            }

            return results;
        }
    }
}
