﻿using System.Collections.Generic;
using System.Data;
 using System.Linq;
 using FluentMigrator;
 using Marr.Data.QGen;
 using Newtonsoft.Json.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;
 using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(147)]
    public class add_custom_formats : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            //Execute.WithConnection(RenameUrlToBaseUrl);
            if (!Schema.Table("QualityDefinitions").Column("QualityTags").Exists())
            {
                Alter.Table("QualityDefinitions").AddColumn("QualityTags").AsString().Nullable().WithDefaultValue(null)
                    .AddColumn("ParentQualityDefinitionId").AsInt64().Nullable().WithDefaultValue(null);
            }

            if (Schema.Table("QualityDefinitions").Index("IX_QualityDefinitions_Quality").Exists())
            {
                Alter.Table("QualityDefinitions").AlterColumn("Quality").AsInt64().Nullable();
                Delete.Index("IX_QualityDefinitions_Quality").OnTable("QualityDefinitions");
            }

            Execute.WithConnection(AddQualityTagsToDefinitions);
            Execute.WithConnection(UpdateProfilesForQualityDefinitions);
            Execute.WithConnection(ConvertQualityModels);
        }

        private void AddQualityTagsToDefinitions(IDbConnection conn, IDbTransaction tran)
        {
            foreach (var definition in QualityDefinition.DefaultQualityDefinitions)
            {
                using (var updateDefCmd = conn.CreateCommand())
                {
                    updateDefCmd.Transaction = tran;
                    updateDefCmd.CommandText = "UPDATE QualityDefinitions SET QualityTags = ? WHERE Quality = ?";
                    updateDefCmd.AddParameter(definition.QualityTags.Select(t => t.Raw).ToJson());
                    updateDefCmd.AddParameter((int)definition.Quality);

                    updateDefCmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateProfilesForQualityDefinitions(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater70(conn, tran);
            updater.UpdateQualityToQualityDefinition();
            updater.Commit();
        }

        private void ConvertQualityModels(IDbConnection conn, IDbTransaction tran)
        {
            var qualityToQualityDefinitions = new Dictionary<int, int>();
            using (var qualityDefCommand = conn.CreateCommand())
            {
                qualityDefCommand.Transaction = tran;
                qualityDefCommand.CommandText = @"SELECT Quality, Id FROM QualityDefinitions";

                using (var qualityDefReader = qualityDefCommand.ExecuteReader())
                {
                    while (qualityDefReader.Read())
                    {
                        qualityToQualityDefinitions[qualityDefReader.GetInt32(0)] = qualityDefReader.GetInt32(1);
                    }
                }
            }

            ConvertQualityModelsOnTable(conn, tran, "MovieFiles", qualityToQualityDefinitions);
            ConvertQualityModelsOnTable(conn, tran, "Blacklist", qualityToQualityDefinitions);
            ConvertQualityModelsOnTable(conn, tran, "History", qualityToQualityDefinitions);
        }

        private void ConvertQualityModelsOnTable(IDbConnection conn, IDbTransaction tran, string tableName, IDictionary<int, int> qualityToQualityDefinitions)
        {
            var qualitiesToUpdate = new Dictionary<string, string>();

            using (IDbCommand qualityModelCmd = conn.CreateCommand())
            {
                qualityModelCmd.Transaction = tran;
                qualityModelCmd.CommandText = @"SELECT Distinct Quality FROM " + tableName;

                using (IDataReader qualityModelReader = qualityModelCmd.ExecuteReader())
                {
                    while (qualityModelReader.Read())
                    {
                        var qualityJson = qualityModelReader.GetString(0);

                        QualityModel147 quality;

                        if (!Json.TryDeserialize<QualityModel147>(qualityJson, out quality))
                        {
                            continue;
                        }

                        if (!quality.Quality.HasValue)
                        {
                            _logger.Error("Found a QualityModel on Table {0} with no actual Quality (JSON: {1})", tableName, qualityJson);
                            continue;
                        }

                        if (qualityToQualityDefinitions.Count == 0)
                        {
                            _logger.Error("Didn't find any quality definitions!. Couldn't upgarde existing quality models! Maybe your db is corrupt?");
                            continue;
                        }

                        if (!qualityToQualityDefinitions.ContainsKey(quality.Quality.Value))
                        {
                            _logger.Error(
                                "Didn't find quality with id {0} among the quality definitions! Couldn't upgrade quality model: {1}. Maybe you deleted it in the db by mistake?",
                                quality.Quality, qualityJson);
                            continue;
                        }

                        var newQualityModel = new QualityModel147
                        {
                            Quality = quality.Quality,
                            Revision = quality.Revision,
                            QualityDefinition = qualityToQualityDefinitions[quality.Quality.Value],
                            HardcodedSubs = quality.HardcodedSubs
                        };

                        var newQualityJson = newQualityModel.ToJson();

                        qualitiesToUpdate.Add(qualityJson, newQualityJson);
                    }
                }
            }

            foreach (var quality in qualitiesToUpdate)
            {
                using (IDbCommand updateCmd = conn.CreateCommand())
                {
                    updateCmd.Transaction = tran;
                    updateCmd.CommandText = "UPDATE " + tableName + " SET Quality = ? WHERE Quality = ?";
                    updateCmd.AddParameter(quality.Value);
                    updateCmd.AddParameter(quality.Key);

                    updateCmd.ExecuteNonQuery();
                }
            }
        }
    }



    public class QualityDefinition147
    {
        public int Id { get; set; }
        public int Quality { get; set; }
        public string Title { get; set; }
        public int? MinSize { get; set; }
        public int? MaxSize { get; set; }
        public List<string> QualityTags { get; set; }
        public int? ParentQualityDefinitionId { get; set; }
    }

    public class QualityModel147
    {
        public int? Quality { get; set; }
        public int? QualityDefinition { get; set; }
        /*public Resolution Resolution { get; set; }
        public Source Source { get; set; }
        public Modifier Modifier { get; set; }*/
        public Revision Revision { get; set; }
        public string HardcodedSubs { get; set; }
    }
}
