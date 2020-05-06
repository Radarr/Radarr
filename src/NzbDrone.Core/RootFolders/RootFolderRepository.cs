using System.Collections.Generic;
using System.Text.Json;
using Dapper;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books.Calibre;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.RootFolders
{
    public interface IRootFolderRepository : IBasicRepository<RootFolder>
    {
    }

    public class RootFolderRepository : BasicRepository<RootFolder>, IRootFolderRepository
    {
        public RootFolderRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        protected override bool PublishModelEvents => true;

        protected override List<RootFolder> Query(SqlBuilder builder)
        {
            var type = typeof(RootFolder);
            var sql = builder.Select(type).AddSelectTemplate(type);

            var results = new List<RootFolder>();

            using (var conn = _database.OpenConnection())
            using (var reader = conn.ExecuteReader(sql.RawSql, sql.Parameters))
            {
                var parser = reader.GetRowParser<RootFolder>(type);
                var settingsIndex = reader.GetOrdinal(nameof(RootFolder.CalibreSettings));
                var serializerSettings = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                while (reader.Read())
                {
                    var body = reader.IsDBNull(settingsIndex) ? null : reader.GetString(settingsIndex);
                    var item = parser(reader);

                    if (body.IsNotNullOrWhiteSpace())
                    {
                        item.CalibreSettings = JsonSerializer.Deserialize<CalibreSettings>(body, serializerSettings);
                    }

                    results.Add(item);
                }
            }

            return results;
        }

        public new void Delete(int id)
        {
            var model = Get(id);
            base.Delete(id);
            ModelDeleted(model);
        }
    }
}
