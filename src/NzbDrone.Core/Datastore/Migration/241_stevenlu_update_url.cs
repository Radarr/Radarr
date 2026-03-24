using System.Collections.Generic;
using System.Data;
using Dapper;
using FluentMigrator;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(241)]
    public class stevenlu_update_url : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(FixStevenLuListsLink);
        }

        private void FixStevenLuListsLink(IDbConnection conn, IDbTransaction tran)
        {
            var updated = new List<object>();

            using (var getStevenLuListCmd = conn.CreateCommand())
            {
                getStevenLuListCmd.Transaction = tran;
                getStevenLuListCmd.CommandText = "SELECT \"Id\", \"Settings\" FROM \"ImportLists\" WHERE \"ConfigContract\" = 'StevenLuSettings'";

                using var reader = getStevenLuListCmd.ExecuteReader();

                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var settings = Json.Deserialize<JObject>(reader.GetString(1));

                    var link = settings.Value<string>("link");

                    if (link.IsNotNullOrWhiteSpace() && link.StartsWith("https://s3.amazonaws.com/popular-movies"))
                    {
                        settings["link"] = "https://popular-movies-data.stevenlu.com/movies.json";
                    }

                    updated.Add(new
                    {
                        Id = id,
                        Settings = settings.ToJson()
                    });
                }
            }

            var updateSql = "UPDATE \"ImportLists\" SET \"Settings\" = @Settings WHERE \"Id\" = @Id";
            conn.Execute(updateSql, updated, transaction: tran);
        }
    }
}
