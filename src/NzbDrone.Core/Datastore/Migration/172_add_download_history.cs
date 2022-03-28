using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(172)]
    public class add_download_history : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("DownloadHistory")
                  .WithColumn("EventType").AsInt32().NotNullable()
                  .WithColumn("MovieId").AsInt32().NotNullable()
                  .WithColumn("DownloadId").AsString().NotNullable()
                  .WithColumn("SourceTitle").AsString().NotNullable()
                  .WithColumn("Date").AsDateTime().NotNullable()
                  .WithColumn("Protocol").AsInt32().Nullable()
                  .WithColumn("IndexerId").AsInt32().Nullable()
                  .WithColumn("DownloadClientId").AsInt32().Nullable()
                  .WithColumn("Release").AsString().Nullable()
                  .WithColumn("Data").AsString().Nullable();

            Create.Index().OnTable("DownloadHistory").OnColumn("EventType");
            Create.Index().OnTable("DownloadHistory").OnColumn("MovieId");
            Create.Index().OnTable("DownloadHistory").OnColumn("DownloadId");

            IfDatabase("sqlite").Execute.WithConnection(InitialImportedDownloadHistory);
        }

        private static readonly Dictionary<int, int> EventTypeMap = new Dictionary<int, int>()
        {
            { 1, 1 }, // MovieHistoryType.Grabbed -> DownloadHistoryType.Grabbed
            { 3, 2 }, // MovieHistoryType.DownloadFolderImported -> DownloadHistoryType.DownloadImported
            { 4, 3 }, // MovieHistoryType.DownloadFailed -> DownloadHistoryType.DownloadFailed
            { 9, 4 } // MovieHistoryType.DownloadIgnored -> DownloadHistoryType.DownloadIgnored
        };

        private void InitialImportedDownloadHistory(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"MovieId\", \"DownloadId\", \"EventType\", \"SourceTitle\", \"Date\", \"Data\" FROM \"History\" WHERE \"DownloadId\" IS NOT NULL AND \"EventType\" IN (1, 3, 4, 9) GROUP BY \"EventType\", \"DownloadId\"";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var movieId = reader.GetInt32(0);
                        var downloadId = reader.GetString(1);
                        var eventType = reader.GetInt32(2);
                        var sourceTitle = reader.GetString(3);
                        var date = reader.GetDateTime(4);
                        var rawData = reader.GetString(5);
                        var data = Json.Deserialize<Dictionary<string, string>>(rawData);

                        var downloadHistoryEventType = EventTypeMap[eventType];
                        var protocol = data.ContainsKey("protocol") ? Convert.ToInt32(data["protocol"]) : (int?)null;
                        var downloadHistoryData = new Dictionary<string, string>();

                        if (data.ContainsKey("indexer"))
                        {
                            downloadHistoryData.Add("indexer", data["indexer"]);
                        }

                        if (data.ContainsKey("downloadClient"))
                        {
                            downloadHistoryData.Add("downloadClient", data["downloadClient"]);
                        }

                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            if (conn.GetType().FullName ==  "Npgsql.NpgsqlConnection")
                            {
                                updateCmd.CommandText = @"INSERT INTO ""DownloadHistory"" (""EventType"", ""MovieId"", ""DownloadId"", ""SourceTitle"", ""Date"", ""Protocol"", ""Data"") VALUES ($1, $2, $3, $4, $5, $6, $7)";
                            }
                            else
                            {
                                updateCmd.CommandText = @"INSERT INTO ""DownloadHistory"" (""EventType"", ""MovieId"", ""DownloadId"", ""SourceTitle"", ""Date"", ""Protocol"", ""Data"") VALUES (?, ?, ?, ?, ?, ?, ?)";
                            }

                            updateCmd.AddParameter(downloadHistoryEventType);
                            updateCmd.AddParameter(movieId);
                            updateCmd.AddParameter(downloadId);
                            updateCmd.AddParameter(sourceTitle);
                            updateCmd.AddParameter(date);
                            updateCmd.AddParameter(protocol);
                            updateCmd.AddParameter(downloadHistoryData.ToJson());

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
