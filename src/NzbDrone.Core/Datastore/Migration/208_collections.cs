using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(208)]
    public class collections : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Collections")
                .WithColumn("TmdbId").AsInt32().Unique()
                .WithColumn("QualityProfileId").AsInt32()
                .WithColumn("RootFolderPath").AsString()
                .WithColumn("MinimumAvailability").AsInt32()
                .WithColumn("SearchOnAdd").AsBoolean()
                .WithColumn("Title").AsString()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("CleanTitle").AsString()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Images").AsString().WithDefaultValue("[]")
                .WithColumn("Monitored").AsBoolean().WithDefaultValue(false)
                .WithColumn("LastInfoSync").AsDateTime().Nullable()
                .WithColumn("Added").AsDateTime().Nullable();

            Alter.Table("MovieMetadata").AddColumn("CollectionTmdbId").AsInt32().Nullable()
                                        .AddColumn("CollectionTitle").AsString().Nullable();

            Alter.Table("ImportLists").AddColumn("Monitor").AsInt32().Nullable();

            Execute.WithConnection(MigrateCollections);
            Execute.WithConnection(MigrateCollectionMonitorStatus);
            Execute.WithConnection(MapCollections);
            Execute.WithConnection(MigrateListMonitor);

            Alter.Table("ImportLists").AlterColumn("Monitor").AsInt32().NotNullable();

            Delete.Column("ShouldMonitor").FromTable("ImportLists");
            Delete.FromTable("ImportLists").Row(new { Implementation = "TMDbCollectionImport" });
            Delete.Column("Collection").FromTable("MovieMetadata");
        }

        private void MigrateCollections(IDbConnection conn, IDbTransaction tran)
        {
            var rootPaths = new List<string>();
            using (var getRootFolders = conn.CreateCommand())
            {
                getRootFolders.Transaction = tran;
                getRootFolders.CommandText = @"SELECT ""Path"" FROM ""RootFolders""";

                using (var definitionsReader = getRootFolders.ExecuteReader())
                {
                    while (definitionsReader.Read())
                    {
                        string path = definitionsReader.GetString(0);
                        rootPaths.Add(path);
                    }
                }
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Collection\", \"ProfileId\", \"MinimumAvailability\", \"Path\" FROM \"Movies\" JOIN \"MovieMetadata\" ON \"Movies\".\"MovieMetadataId\" = \"MovieMetadata\".\"Id\" WHERE \"Collection\" IS NOT NULL GROUP BY \"Collection\"";

                var addedCollections = new List<int>();
                var added = DateTime.UtcNow;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var collection = reader.GetString(0);
                        var qualityProfileId = reader.GetInt32(1);
                        var minimumAvailability = reader.GetInt32(2);
                        var moviePath = reader.GetString(3);
                        var data = STJson.Deserialize<MovieCollection206>(collection);

                        if (addedCollections.Contains(data.TmdbId))
                        {
                            continue;
                        }

                        var rootFolderPath = rootPaths.Where(r => r.IsParentPath(moviePath))
                                          .OrderByDescending(r => r.Length)
                                          .FirstOrDefault();

                        if (rootFolderPath == null)
                        {
                            rootFolderPath = rootPaths.FirstOrDefault();
                        }

                        if (rootFolderPath == null)
                        {
                            rootFolderPath = moviePath.GetParentPath();
                        }

                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = @"INSERT INTO ""Collections"" (""TmdbId"", ""Title"", ""CleanTitle"", ""SortTitle"", ""Added"", ""QualityProfileId"", ""RootFolderPath"", ""SearchOnAdd"", ""MinimumAvailability"") VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";
                            updateCmd.AddParameter(data.TmdbId);
                            updateCmd.AddParameter(data.Name);
                            updateCmd.AddParameter(data.Name.CleanMovieTitle());
                            updateCmd.AddParameter(Parser.Parser.NormalizeTitle(data.Name));
                            updateCmd.AddParameter(added);
                            updateCmd.AddParameter(qualityProfileId);
                            updateCmd.AddParameter(rootFolderPath);
                            updateCmd.AddParameter(true);
                            updateCmd.AddParameter(minimumAvailability);

                            updateCmd.ExecuteNonQuery();
                        }

                        addedCollections.Add(data.TmdbId);
                    }
                }
            }
        }

        private void MigrateCollectionMonitorStatus(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Enabled\", \"EnableAuto\", \"Settings\", \"ShouldMonitor\", \"Id\" FROM \"ImportLists\" WHERE \"Implementation\" = \"TMDbCollectionImport\"";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var enabled = reader.GetBoolean(0);
                        var enabledAutoAdd = reader.GetBoolean(1);
                        var settings = reader.GetString(2);
                        var shouldMonitor = reader.GetBoolean(3);
                        var listId = reader.GetInt32(4);
                        var data = STJson.Deserialize<TmdbCollectionSettings206>(settings);

                        if (!enabled || !enabledAutoAdd || !int.TryParse(data.CollectionId, out var collectionId))
                        {
                            continue;
                        }

                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = @"UPDATE ""Collections"" SET ""Monitored"" = ? WHERE ""TmdbId"" = ?";
                            updateCmd.AddParameter(true);
                            updateCmd.AddParameter(collectionId);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private void MigrateListMonitor(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"ShouldMonitor\", \"Id\" FROM \"ImportLists\"";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var shouldMonitor = reader.GetBoolean(0);
                        var listId = reader.GetInt32(1);

                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = @"UPDATE ""ImportLists"" SET ""Monitor"" = ? WHERE ""Id"" = ?";
                            updateCmd.AddParameter(shouldMonitor ? 0 : 2);
                            updateCmd.AddParameter(listId);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private void MapCollections(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"Collection\" FROM \"MovieMetadata\" WHERE \"Collection\" IS NOT NULL";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var collection = reader.GetString(1);
                        var data = STJson.Deserialize<MovieCollection206>(collection);

                        var collectionId = data.TmdbId;
                        var collectionTitle = data.Name;

                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = @"UPDATE ""MovieMetadata"" SET ""CollectionTmdbId"" = ?, ""CollectionTitle"" = ? WHERE ""Id"" = ?";
                            updateCmd.AddParameter(collectionId);
                            updateCmd.AddParameter(collectionTitle);
                            updateCmd.AddParameter(id);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private class MovieCollection206
        {
            public string Name { get; set; }
            public int TmdbId { get; set; }
        }

        private class MovieCollection207
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public int TmdbId { get; set; }
        }

        private class TmdbCollectionSettings206
        {
            public string CollectionId { get; set; }
        }
    }
}
