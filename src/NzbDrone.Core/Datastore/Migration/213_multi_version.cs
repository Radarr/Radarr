using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(213)]
    public class multi_version : NzbDroneMigrationBase
    {
        private readonly JsonSerializerOptions _serializerSettings;

        public multi_version()
        {
            _serializerSettings = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        protected override void MainDbUpgrade()
        {
            Alter.Table("Movies").AddColumn("QualityProfileIds").AsString().WithDefaultValue("[]");
            Alter.Table("ImportLists").AddColumn("QualityProfileIds").AsString().WithDefaultValue("[]");
            Alter.Table("Collections").AddColumn("QualityProfileIds").AsString().WithDefaultValue("[]");

            Execute.WithConnection(MigrateProfileIds);
            Execute.WithConnection(MigrateListProfileIds);
            Execute.WithConnection(MigrateCollectionProfileIds);

            Delete.Column("ProfileId").Column("MovieFileId").FromTable("Movies");
            Delete.Column("ProfileId").FromTable("ImportLists");
            Delete.Column("QualityProfileId").FromTable("Collections");
        }

        private void MigrateProfileIds(IDbConnection conn, IDbTransaction tran)
        {
            var movies = new List<Movie209>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"ProfileId\" FROM \"Movies\"";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var profileId = reader.GetInt32(1);

                        movies.Add(new Movie209
                        {
                            Id = id,
                            QualityProfileIds = JsonSerializer.Serialize(new List<int> { profileId }, _serializerSettings)
                        });
                    }
                }
            }

            var updateSql = "UPDATE \"Movies\" SET \"QualityProfileIds\" = @QualityProfileIds WHERE \"Id\" = @Id";
            conn.Execute(updateSql, movies, transaction: tran);
        }

        private void MigrateListProfileIds(IDbConnection conn, IDbTransaction tran)
        {
            var movies = new List<Movie209>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"ProfileId\" FROM \"ImportLists\"";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var profileId = reader.GetInt32(1);

                        movies.Add(new Movie209
                        {
                            Id = id,
                            QualityProfileIds = JsonSerializer.Serialize(new List<int> { profileId }, _serializerSettings)
                        });
                    }
                }
            }

            var updateSql = "UPDATE \"ImportLists\" SET \"QualityProfileIds\" = @QualityProfileIds WHERE \"Id\" = @Id";
            conn.Execute(updateSql, movies, transaction: tran);
        }

        private void MigrateCollectionProfileIds(IDbConnection conn, IDbTransaction tran)
        {
            var movies = new List<Movie209>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"QualityProfileId\" FROM \"Collections\"";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var profileId = reader.GetInt32(1);

                        movies.Add(new Movie209
                        {
                            Id = id,
                            QualityProfileIds = JsonSerializer.Serialize(new List<int> { profileId }, _serializerSettings)
                        });
                    }
                }
            }

            var updateSql = "UPDATE \"Collections\" SET \"QualityProfileIds\" = @QualityProfileIds WHERE \"Id\" = @Id";
            conn.Execute(updateSql, movies, transaction: tran);
        }

        private class Movie208
        {
            public int Id { get; set; }
            public int ProfileId { get; set; }
        }

        private class Movie209
        {
            public int Id { get; set; }
            public string QualityProfileIds { get; set; }
        }
    }
}
